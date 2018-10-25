using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System.Threading;
using System.Diagnostics;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Key privateKey = new Key();
            PubKey publicKey = privateKey.PubKey;
            textBox1.AppendText("PubKey：" + publicKey.ToString() + Environment.NewLine);
            textBox1.AppendText("Main：" + publicKey.GetAddress(Network.Main) + Environment.NewLine);
            textBox1.AppendText("Test：" + publicKey.GetAddress(Network.TestNet) + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);

            var publicKeyHash = publicKey.Hash;
            textBox1.AppendText("PubKeyHash：" + publicKeyHash + Environment.NewLine);
            textBox1.AppendText("Main：" + publicKeyHash.GetAddress(Network.Main) + Environment.NewLine);
            textBox1.AppendText("Test：" + publicKeyHash.GetAddress(Network.TestNet) + Environment.NewLine);
            textBox1.AppendText("Main.ScriptPubKey：" + publicKeyHash.GetAddress(Network.Main).ScriptPubKey + Environment.NewLine);
            textBox1.AppendText("Test.ScriptPubKey：" + publicKeyHash.GetAddress(Network.TestNet).ScriptPubKey + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);

            var paymentScript = publicKeyHash.ScriptPubKey;
            var sameMainNetAddress = paymentScript.GetDestinationAddress(Network.Main);
            // mainNetAddress is sameMainNetAddress!

            var samePublicKeyHash = (KeyId)paymentScript.GetDestination();
            var sameMainNetAddress2 = new BitcoinPubKeyAddress(samePublicKeyHash, Network.Main);
            // mainNetAddress is sameMainNetAddress2!

            BitcoinSecret mainNetPrivateKey = privateKey.GetBitcoinSecret(Network.Main);
            BitcoinSecret testNetPrivateKey = privateKey.GetBitcoinSecret(Network.TestNet);
            textBox1.AppendText("Main.PrivateKey：" + mainNetPrivateKey + Environment.NewLine);
            textBox1.AppendText("Test.PrivateKey：" + testNetPrivateKey + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);
            // mainNetPrivateKey is privateKey.GetWif(Network.Main);

            QBitNinjaClient client = new QBitNinjaClient(Network.Main);
            var transactionId = uint256.Parse("f13dc48fb035bbf0a6e989a26b3ecb57b84f85e0836e777d6edf60d87a4a2d94");
            GetTransactionResponse transactionResponse = client.GetTransaction(transactionId).Result;
            Transaction transaction = transactionResponse.Transaction;
            textBox1.AppendText("TransactionId：" + transactionResponse.TransactionId + Environment.NewLine);
            textBox1.AppendText("transaction.GetHash：" + transaction.GetHash() + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);

            #region
            List<ICoin> receivedCoins = transactionResponse.ReceivedCoins;
            foreach(var coin in receivedCoins)
            {
                Money amount = (Money)coin.Amount;
                textBox1.AppendText("amount.ToDecimal(MoneyUnit.BTC)：" + amount.ToDecimal(MoneyUnit.BTC) + Environment.NewLine);

                paymentScript = coin.TxOut.ScriptPubKey;
                textBox1.AppendText("coin.TxOut.ScriptPubKey：" + paymentScript + Environment.NewLine);

                var address = paymentScript.GetDestinationAddress(Network.Main);
                textBox1.AppendText("paymentScript.GetDestinationAddress：" + address + Environment.NewLine);
            }
            textBox1.AppendText(Environment.NewLine);
            #endregion

            #region
            var outputs = transaction.Outputs;
            foreach(TxOut output in outputs)
            {
                Money amount = output.Value;
                textBox1.AppendText("amount.ToDecimal(MoneyUnit.BTC)：" + amount.ToDecimal(MoneyUnit.BTC) + Environment.NewLine);

                var address = paymentScript.GetDestinationAddress(Network.Main);
                textBox1.AppendText("paymentScript.GetDestinationAddress：" + address + Environment.NewLine);
            }
            textBox1.AppendText(Environment.NewLine);
            #endregion

            var inputs = transaction.Inputs;
            foreach(TxIn input in inputs)
            {
                OutPoint previousOutpoint = input.PrevOut;
                textBox1.AppendText("previousOutpoint.Hash：" + previousOutpoint.Hash + Environment.NewLine);
                textBox1.AppendText("previousOutpoint.N：" + previousOutpoint.N + Environment.NewLine);
            }
            textBox1.AppendText(Environment.NewLine);

            Money twentyOneBtc = new Money(21, MoneyUnit.BTC);
            var scriptPubKey = transaction.Outputs.First().ScriptPubKey;
            TxOut txOut = new TxOut(twentyOneBtc, scriptPubKey);

            OutPoint firstOutpoint = receivedCoins.First().Outpoint;
            textBox1.AppendText("firstOutpoint.Hash：" + firstOutpoint.Hash + Environment.NewLine);
            textBox1.AppendText("firstOutpoint.N：" + firstOutpoint.N + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);

            OutPoint firstPreviousOutPoint = transaction.Inputs.First().PrevOut;
            var firstPreviousTransaction = client.GetTransaction(firstPreviousOutPoint.Hash).Result.Transaction;
            textBox1.AppendText("firstPreviousTransaction.IsCoinBase：" + firstPreviousTransaction.IsCoinBase + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);

            Money spentAmount = Money.Zero;
            var spentCoins = receivedCoins;//とりあえず付け足し
            foreach (var spentCoin in spentCoins)
            {
                spentAmount = (Money)spentCoin.Amount.Add(spentAmount);
                textBox1.AppendText("spentAmount.ToDecimal(MoneyUnit.BTC)：" + spentAmount.ToDecimal(MoneyUnit.BTC) + Environment.NewLine);
            }

            // feeがnullで飛ぶ。
            //var fee = transaction.GetFee(spentCoins.ToArray());
            //textBox1.AppendText("fee：" + fee.ToString() + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //https://programmingblockchain.gitbooks.io/programmingblockchain-japanese/content/bitcoin_transfer/spend_your_coin.html

            var network = Network.Main;
            var privateKey = new Key();
            var bitcoinPrivateKey = privateKey.GetWif(network);
            var address = bitcoinPrivateKey.GetAddress();
            textBox1.AppendText("bitcoinPrivateKey.GetAddress：" + address + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);

            bitcoinPrivateKey = new BitcoinSecret("cSZjE4aJNPpBtU6xvJ6J4iBzDgTmzTjbq8w2kqnYvAprBCyTsG4x");
            network = bitcoinPrivateKey.Network;
            address = bitcoinPrivateKey.GetAddress();
            textBox1.AppendText("bitcoinPrivateKey.Network：" + network + Environment.NewLine);
            textBox1.AppendText("bitcoinPrivateKey.GetAddress：" + address + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);

            var client = new QBitNinjaClient(network);
            var transactionId = uint256.Parse("e44587cf08b4f03b0e8b4ae7562217796ec47b8c91666681d71329b764add2e3");
            var transactionResponse = client.GetTransaction(transactionId).Result;
            textBox1.AppendText("transactionResponse.TransactionId：" + transactionResponse.TransactionId + Environment.NewLine);
            textBox1.AppendText("transactionResponse.Block.Confirmations：" + transactionResponse.Block.Confirmations + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);

            var receivedCoins = transactionResponse.ReceivedCoins;
            OutPoint outPointToSpend = null;
            foreach(var coin in receivedCoins)
            {
                if(coin.TxOut.ScriptPubKey == bitcoinPrivateKey.ScriptPubKey)
                {
                    outPointToSpend = coin.Outpoint;
                }
            }
            if (outPointToSpend == null)
                throw new Exception("エラー：どのトランザクションアウトプットも送ったコインのScriptPubKeyを持ってない！");
            textBox1.AppendText("使いたいアウトポイントの番号：" + (outPointToSpend.N+1).ToString() + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);

            var transaction = new Transaction();
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outPointToSpend
            });

            var halloOfTheMakersAddress = new BitcoinPubKeyAddress("1KF8kUVHK42XzgcmJF4Lxz4wcL5WDL97PB");
            TxOut halloOfTheMakersTxOut = new TxOut()
            {
                Value = new Money((decimal)0.5, MoneyUnit.BTC),
                ScriptPubKey = halloOfTheMakersAddress.ScriptPubKey
            };
            TxOut changeBackTxOut = new TxOut()
            {
                Value = new Money((decimal)0.4999, MoneyUnit.BTC),
                ScriptPubKey = bitcoinPrivateKey.ScriptPubKey
            };
            transaction.Outputs.Add(halloOfTheMakersTxOut);
            transaction.Outputs.Add(changeBackTxOut);

            var halloOfTheMakersAmount = new Money(0.5m, MoneyUnit.BTC);
            var minerFee = new Money(0.0001m, MoneyUnit.BTC);
            var txInAmount = receivedCoins[(int)outPointToSpend.N].TxOut.Value;
            Money changeBackAmount = txInAmount - halloOfTheMakersAmount - minerFee;

            halloOfTheMakersTxOut = new TxOut()
            {
                Value = halloOfTheMakersAmount,
                ScriptPubKey = halloOfTheMakersAddress.ScriptPubKey
            };
            changeBackTxOut = new TxOut()
            {
                Value = changeBackAmount,
                ScriptPubKey = bitcoinPrivateKey.ScriptPubKey
            };
            transaction.Outputs.Add(halloOfTheMakersTxOut);
            transaction.Outputs.Add(changeBackTxOut);

            var message = "nopara73 loves NBitcoin!";
            var bytes = Encoding.UTF8.GetBytes(message);
            transaction.Outputs.Add(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
            });

            //
            //この辺り不明点多数。
            //

            BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;
            if (broadcastResponse.Success)
            {
                textBox1.AppendText("Success! You can check out the hash of the transaciton in any block explorer:" + Environment.NewLine);
                textBox1.AppendText(transaction.GetHash() + Environment.NewLine);
                textBox1.AppendText(Environment.NewLine);
            }
            else
            {
                textBox1.AppendText("Error:" + Environment.NewLine);
                textBox1.AppendText(Environment.NewLine);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //https://programmingblockchain.gitbooks.io/programmingblockchain-japanese/content/bitcoin_transfer/proof_of_ownership_as_an_authentication_method.html

            var message = "I am Craig Wright";
            var signature = "IN5v9+3HGW1q71OqQ1boSZTm0/DCiMpI8E4JB1nD67TCbIVMRk/e3KrTT9GvOuu3NGN0w8R2lWOV2cxnBp+Of8c=";

            var address = new BitcoinPubKeyAddress("1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa");
            bool isCraigWrightSatoshi = address.VerifyMessage(message, signature);
            textBox1.AppendText("Is Craig Wright Satoshi? " + isCraigWrightSatoshi + Environment.NewLine);
            textBox1.AppendText(Environment.NewLine);
        }

        double AttackerSuccessProbability(double q, int z)
        {
            double p = 1.0 - q;
            double lambda = z * (q / p);
            double sum = 1.0;

            int i, k;
            for(k = 1; k <= z; k++)
            {
                double poisson = System.Math.Exp(-lambda);
                for(i = 1; i <= k; i++)
                {
                    poisson *= lambda / i;
                    sum -= poisson * (1 - System.Math.Pow(q / p, z - k));
                }
            }

            return sum;
        }

        private bool run = false;
        private Thread thread = null;
        private void button_start_Click(object sender, EventArgs e)
        {
            object[] param = new object[2];
            param[0] = 123;
            param[1] = "ABC";

            run = true;
            thread = new Thread(new ParameterizedThreadStart(runThread));
            thread.Start(param);
        }
        private void runThread(object param)
        {
            object[] prm = (object[])param;

            Process proc = new Process();
            proc.StartInfo.FileName = "C:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\Python36_64\\python.exe";
            //proc.StartInfo.Arguments = string.Format("-u sabachan.py");
            proc.StartInfo.Arguments = string.Format("-u sabachan.py {0} {1}", prm[0], prm[1]);

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;

            proc.Start();

            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.BeginOutputReadLine();

            while (run)
            {
                Thread.Sleep(100);
            }

            proc.Kill();
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            textBox1.AppendText(e.Data + Environment.NewLine);
        }

        private void button_end_Click(object sender, EventArgs e)
        {
            run = false;
            thread.Join();
        }
    }
}
