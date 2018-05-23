using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhantasmaAudit
{
    class Program
    {
        static void Main(string[] args)
        {
            uint startBlock = 2298101;
            uint endblock = 2299480;

            string transactions_output_filename = "phantasma_transactions.csv";
            string total_output_filename = "phantasma_totals.csv";

            var soul_scripthash = new UInt160(LuxUtils.ReverseHex("4b4f63919b9ecfd2483f0c72ff46ed31b5bbb7a4").HexToBytes());

            Console.WriteLine("Generating sale audit. This can take several minutes, please be patient.");

            var start_time = Environment.TickCount;

            var api = new LocalRPCNode(10332, "http://neoscan.io");

            var blockCount = api.GetBlockHeight();

            var balances = new Dictionary<string, decimal>();

            var total_output = new List<string>();
            var transaction_output = new List<string>();

            var extra_refunds = new Dictionary<string, decimal>();

            transaction_output.Add($"Tx type,Tx hash,Address,NEO sent");
            for (uint height = startBlock; height<=endblock; height++)            
            {
                var block = api.GetBlock(height);

                foreach (var tx in block.transactions)
                {
                    foreach (var output in tx.outputs)
                    {
                        if (output.scriptHash == soul_scripthash)
                        {
                            Transaction other = null;
                            foreach (var input in tx.inputs)
                            {
                                other = api.GetTransaction(input.prevHash);
                                UInt160 src = other.outputs[input.prevIndex].scriptHash;

                                var src_addr = src.ToAddress();
                                var ss = tx.type.ToString().Replace("Transaction", "");

                                var balance = balances.ContainsKey(src_addr) ? balances[src_addr] : 0;

                                if (tx.type == TransactionType.InvocationTransaction)
                                {
                                    balance += output.value;
                                }
                                else
                                {
                                    var extra = extra_refunds.ContainsKey(src_addr) ? extra_refunds[src_addr] : 0;
                                    extra += output.value;
                                    extra_refunds[src_addr] = extra;
                                }

                                balances[src_addr] = balance;

                                transaction_output.Add($"{ss},{tx.Hash},{src_addr},{output.value}");

                                break;
                            }


                            break;
                        }
                    }

                }
            }

            total_output.Add($"Address,SOUL received,NEO sent,NEO to be refunded");
            foreach (KeyValuePair<string, decimal> entry in balances.OrderBy(x => x.Value))
            {
                var refund_amount = entry.Value > 10 ? entry.Value - 10 : 0;
                var token_amount = (entry.Value > 10 ? 10 : entry.Value) * 273;

                if (extra_refunds.ContainsKey(entry.Key))
                {
                    refund_amount += extra_refunds[entry.Key];
                }

                total_output.Add($"{entry.Key},{token_amount},{entry.Value},{refund_amount}");
            }

            File.WriteAllLines(total_output_filename, total_output.ToArray());
            File.WriteAllLines(transactions_output_filename, transaction_output.ToArray());

            var total_blocks = (endblock - startBlock) + 1;

            var end_time = Environment.TickCount;
            var delta = (end_time - start_time) / 1000;

            Console.WriteLine("Finished in " + delta + " seconds, loaded "+ total_blocks + " blocks");
        }
    }
}
