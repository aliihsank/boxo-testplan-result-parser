
using ClosedXML.Excel;
using System.Data;

namespace BitswapPeerSelectionResultParser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Enter the path of result txt file:");

                    var path = Console.ReadLine();

                    if(path == null)
                    {
                        Console.WriteLine("Path can not be null!!");
                        continue;
                    }

                    if (!path.EndsWith(".txt"))
                    {
                        Console.WriteLine("File path must end with .txt");
                        continue;
                    }

                    var parsedPath = path.Replace(".txt", "-Parsed.txt");

                    var rawContent = File.ReadAllLines(path);

                    var parsedContent = ParseFile(rawContent);

                    WriteResultFile(parsedPath, parsedContent.sentBlockRequests, parsedContent.receivedBlocks);

                    WriteDataTransactionsTable(parsedPath, parsedContent.sentBlockRequests, parsedContent.receivedBlocks);

                    Console.WriteLine("Parsed the results and saved it into: " + parsedPath);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static (string[] sentBlockRequests, string[] receivedBlocks) ParseFile(string[] lines)
        {
            var sentBlockRequestList = new List<string>();
            var receivedBlockList = new List<string>();

            foreach (var line in lines)
            {
                var indexS = line.IndexOf("Sending Want-Block");
                var indexR = line.IndexOf("Received Block response from");
                
                if(indexS >= 0)
                {
                    var blockRequest = line.Substring(indexS);

                    sentBlockRequestList.Add(blockRequest);
                }


                if (indexR >= 0)
                {
                    var receivedBlock = line.Substring(indexR);

                    receivedBlockList.Add(receivedBlock);
                }
            }

            return (sentBlockRequestList.ToArray(), receivedBlockList.ToArray());
        }

        private static void WriteResultFile(string path, string[] sentBlockRequests, string[] receivedBlocks)
        {
            File.WriteAllText(path, "Sent Block Requests\n");

            File.AppendAllLines(path, sentBlockRequests);

            File.AppendAllText(path, "\nReceived Blocks\n");

            File.AppendAllLines(path, receivedBlocks);
        }

        private static void WriteDataTransactionsTable(string path, string[] sentBlockRequests, string[] receivedBlocks)
        {
            XLWorkbook workbook = new XLWorkbook();

            // SentBlockRequests Table
            DataTable dtSentBlockRequests = new DataTable();
            dtSentBlockRequests.Columns.Add("Request Number", typeof(int));
            dtSentBlockRequests.Columns.Add("Block Cid", typeof(string));
            dtSentBlockRequests.Columns.Add("Requested From", typeof(string));

            for(var i = 0; i < sentBlockRequests.Length; i++)
            {
                var sentBlockRow = dtSentBlockRequests.NewRow();
                sentBlockRow["Request Number"] = i + 1;
                sentBlockRow["Block Cid"] = sentBlockRequests[i].Substring(sentBlockRequests[i].IndexOf(", Block:  ") + 10);
                sentBlockRow["Requested From"] = sentBlockRequests[i].Substring(sentBlockRequests[i].IndexOf("Sending Want-Block For Peer:  ") + 30, 52);

                dtSentBlockRequests.Rows.Add(sentBlockRow);
            }

            workbook.Worksheets.Add(dtSentBlockRequests, "SentBlockRequests");


            // ReceivedBlocks Table
            DataTable dtReceivedBlocks = new DataTable();
            dtReceivedBlocks.Columns.Add("Block Number", typeof(int));
            dtReceivedBlocks.Columns.Add("Received From", typeof(string));

            for (var i = 0; i < receivedBlocks.Length; i++)
            {
                var receivedBlockRow = dtReceivedBlocks.NewRow();
                receivedBlockRow["Block Number"] = i + 1;
                receivedBlockRow["Received From"] = receivedBlocks[i].Substring(receivedBlocks[i].IndexOf("Received Block response from:  ") + 31, 52);

                dtReceivedBlocks.Rows.Add(receivedBlockRow);
            }

            workbook.Worksheets.Add(dtReceivedBlocks, "ReceivedBlocks");

            workbook.SaveAs(path.Replace(".txt", "-Transactions.xlsx"));
        }
    }
}
