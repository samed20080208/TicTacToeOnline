using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TicTacToeOnline
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            client = new TcpClient("127.0.0.1", 5000);
            stream = client.GetStream();

            receiveThread = new Thread(ReceiveData);
            receiveThread.Start();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int index = int.Parse(button.Name.Substring(3));
            SendMessage(index.ToString());
        }

        private void SendMessage(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        private void ReceiveData()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int byteCount = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, byteCount);

                if (message.Contains(","))
                {
                    string[] newBoard = message.Split(',');

                    Dispatcher.Invoke(() =>
                    {
                        UpdateBoard(newBoard);
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(message);
                    });
                }
            }
        }

        private void UpdateBoard(string[] newBoard)
        {
            for (int i = 0; i < newBoard.Length; i++)
            {
                GetButton(i).Content = newBoard[i];
                GetButton(i).IsEnabled = string.IsNullOrEmpty(newBoard[i]);
            }
        }

        private Button GetButton(int index)
        {
            return (Button)this.FindName($"Btn{index}");
        }
    }
}