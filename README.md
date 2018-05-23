# SOUL token generation event audit
This repository contains reports of the Phantasma token generation event, which can be found in the folder output.
The reports are provided in .csv format, that can be imported into Excel, Google Docs or other programs.

phantasma_transactions.csv contains a list of all transactions sent to the SOUL scripthash address, ordered by main net block inclusion.
phantasma_totals.csv contains a list of the full addresses that participated in the TGE, and the amount of tokens each one received along with amount of NEO received and NEO that will be refunded.

## Generating the reports yourself
We also provide the C# program used to generate this reports, so you can run itself yourself and confirm the result.

This program requires Neo.Lux library to extract data from the NEO main net.

It is also necessary to run [neo-cli](https://github.com/neo-project/neo-cli/) to act as a local full node, and make sure that is fully synced to the main net before you start the report generation.
