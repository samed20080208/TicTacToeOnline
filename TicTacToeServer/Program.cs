using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TicTacToeServer
{
    class Program
    {
        static TcpClient client1, client2;
        static NetworkStream stream1, stream2;
        static string[] board = new string[9];
        static bool isXTurn = true;  
        static bool gameOver = false;

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Runed");

            client1 = server.AcceptTcpClient();
            Console.WriteLine("P1 joined");

            client2 = server.AcceptTcpClient();
            Console.WriteLine("P2 joined");

            stream1 = client1.GetStream();
            stream2 = client2.GetStream();

            ResetBoard();

            Thread thread1 = new Thread(HandleClient1);
            thread1.Start();

            Thread thread2 = new Thread(HandleClient2);
            thread2.Start();
        }
        static void ResetBoard()
        {
            for (int i = 0; i < 9; i++)
                board[i] = string.Empty;
            isXTurn = true;
            gameOver = false;
        }
        static string CheckForWinner()
        {
            int[,] winningCombinations = new int[,]
            {
                { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 },  
                { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 },  
                { 0, 4, 8 }, { 2, 4, 6 }               
            };

            for (int i = 0; i < winningCombinations.GetLength(0); i++)
            {
                int a = winningCombinations[i, 0];
                int b = winningCombinations[i, 1];
                int c = winningCombinations[i, 2];

                if (!string.IsNullOrEmpty(board[a]) && board[a] == board[b] && board[a] == board[c])
                {
                    return board[a];  
                }
            }

            foreach (var cell in board)
            {
                if (string.IsNullOrEmpty(cell))
                    return null;
            }

            return "Draw";  
        }

        static void HandleClient1()
        {
            while (true)
            {
                if (gameOver) continue;

                byte[] buffer = new byte[1024];
                int byteCount = stream1.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, byteCount);
                int index = int.Parse(message);

                if (board[index] == string.Empty && isXTurn) 
                {
                    board[index] = "X";
                    isXTurn = false;
                    BroadcastBoard();
                    CheckGameState();
                }
            }
        }

        static void HandleClient2()
        {
            while (true)
            {
                if (gameOver) continue;

                byte[] buffer = new byte[1024];
                int byteCount = stream2.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, byteCount);
                int index = int.Parse(message);

                if (board[index] == string.Empty && !isXTurn) 
                {
                    board[index] = "O";
                    isXTurn = true;
                    BroadcastBoard();
                    CheckGameState();
                }
            }
        }

        static void BroadcastBoard()
        {
            string boardState = string.Join(",", board);
            byte[] data = Encoding.ASCII.GetBytes(boardState);

            stream1.Write(data, 0, data.Length);
            stream2.Write(data, 0, data.Length);
        }

        static void CheckGameState()
        {
            string winner = CheckForWinner();
            if (winner != null)
            {
                gameOver = true;
                string message = winner == "Draw" ? "Draw!" : $"{winner} Wins!";
                byte[] data = Encoding.ASCII.GetBytes(message);

                stream1.Write(data, 0, data.Length);
                stream2.Write(data, 0, data.Length);
            }
        }
    }
}