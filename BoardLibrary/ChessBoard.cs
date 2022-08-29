using System;
using System.Collections.Generic;

namespace BoardLibrary
{
    public class ChessBoard
    {
        public int[] board = new int[64];
        public string FEN = "";

        int currentTurn = 1; // Black: 0, White: 1 (default white)
        string castleAvailability = "";

        List<int> WhiteThreats = new List<int>();
        List<int> BlackThreats = new List<int>();

        public enum Piece
        {
            Pawn = 1,
            Knight = 2,
            Bishop = 3,
            Rook = 4,
            Queen = 5,
            King = 6
        }

        public enum Color
        {
            Black = 0,
            White = 1
        }

        Dictionary<char, int> pieceValues = new Dictionary<char, int>()
        {
            ['p'] = 1, ['P'] = 9,
            ['n'] = 2, ['N'] = 10,
            ['b'] = 3, ['B'] = 11,
            ['r'] = 4, ['R'] = 12,
            ['q'] = 5, ['Q'] = 13,
            ['k'] = 6, ['K'] = 14,
            [' '] = 0
        };
        Dictionary<int, char> pieces = new Dictionary<int, char>()
        {
            [1] = 'p', [9] = 'P',
            [2] = 'n', [10] = 'N',
            [3] = 'b', [11] = 'B',
            [4] = 'r', [12] = 'R',
            [5] = 'q', [13] = 'Q',
            [6] = 'k', [14] = 'K',
            [0] = ' '
        };

        public void Draw() // Displays the current board in the console
        {
            Console.Clear();

            for (int i = 0; i < 17; i++)
            {
                if (i == 0)
                {
                    Console.Write("\u250C");
                    for (int j = 0; j < 15; j++)
                    {
                        if (j % 2 == 0) { Console.Write("\u2500\u2500\u2500"); }
                        else { Console.Write("\u252C"); }
                    }
                    Console.WriteLine("\u2510");
                }

                else if (i == 16)
                {
                    Console.Write("\u2514");
                    for (int j = 0; j < 15; j++)
                    {
                        if (j % 2 == 0) { Console.Write("\u2500\u2500\u2500"); }
                        else { Console.Write("\u2534"); }
                    }
                    Console.WriteLine("\u2518");
                }

                else if (i % 2 == 0)
                {
                    Console.Write("\u251C");
                    for (int j = 0; j < 15; j++)
                    {
                        if (j % 2 == 0) { Console.Write("\u2500\u2500\u2500"); }
                        else { Console.Write("\u253C"); }
                    }
                    Console.WriteLine("\u2524");
                }

                else
                {
                    for (int j = 0; j < 18; j++)
                    {
                        if (j % 2 == 0) { Console.Write("\u2502"); }
                        else { Console.Write("   "); }
                    }
                    Console.Write("\n");
                }
            }

            for (int i = 0; i < 64; i++)
            {
                Console.SetCursorPosition(2 + (i % 8) * 4, 15 - (i - (i % 8)) / 4);
                Console.Write(pieces[board[i]]);
            }

            Console.SetCursorPosition(0, 20);
        } 

        public void Setup() // Sets the board layout using the starting FEN string
        {
            string[] fields = FEN.Split(" ");

            int currentPos = 0;
            string[] tempLayout = fields[0].Split("/");
            string[] layout = new string[8];
            for (int i = 0; i < 8; i++) { layout[i] = tempLayout[7 - i]; }
            foreach (string rank in layout)
            {
                foreach (char piece in rank)
                {
                    if (Convert.ToInt32(piece) <= 56 && Convert.ToInt32(piece) >= 49)
                    {
                        currentPos += Convert.ToInt32(Convert.ToString(piece));
                    }
                    else
                    {
                        board[currentPos] = pieceValues[piece];
                        currentPos++;
                    }
                }
            }

            if (fields[1] == "b") { currentTurn = 0; }

            castleAvailability = fields[2];
        }

        public void MoveExact(int from, int to) // Move Square
        {
            if (IsLegal(from, to))
            {
                int piece = board[from];
                board[from] = 0;
                board[to] = piece;
            }
        }

        public void Move(string move)
        {
            switch (move.Length)
            {
                case 2:
                    int pos = Convert.ToInt32(move[0]) - 97;
                    while (pos < 64)
                    {
                        if (board[pos] == pieceValues['p'] + (currentTurn * 8))
                        {
                            MoveExact(pos, AlgToInt(move));
                            pos = 64;
                        }

                        pos += 8;
                    }
                    break;

                case 3:
                    if (move.Contains("-")) // Kingside castle
                    {
                        if (currentTurn == 0 && castleAvailability.Contains("k") && board[61] == 0 && board[62] == 0)
                        {
                            if (true) // Check if blocked by check
                            {
                                MoveExact(60, 62);
                                MoveExact(63, 61);
                            }
                        }
                        else if (currentTurn == 1 && castleAvailability.Contains("K") && board[5] == 0 && board[6] == 0)
                        {
                            if (true) // Check if blocked by check
                            {
                                MoveExact(4, 6);
                                MoveExact(7, 5);
                            }
                        }
                    }

                    else if (move.Contains("+")) // Pawn check
                    {
                        Move(Convert.ToString(move[0]) + Convert.ToString(move[1]));
                    }

                    else // Non-pawn move
                    {
                        int movePos = AlgToInt(Convert.ToString(move[1]) + Convert.ToString(move[2]));
                        int[] moves;
                        switch (move[0])
                        {
                            case 'N': moves = KnightMove(movePos); break;
                            case 'B': moves = BishopMove(movePos); break;
                            case 'R': moves = RookMove(movePos); break;
                            case 'Q': moves = QueenMove(movePos); break;
                            case 'K': moves = KingMove(movePos); break;

                            foreach (int p in moves)
                            {
                                if (pieces[board[p]] == Convert.ToChar(Convert.ToString(move[0]).ToLower()) && currentTurn == 0) { MoveExact(p, movePos); }
                                else if (pieces[board[p]] == move[0] && currentTurn == 1) { MoveExact(p, movePos); }
                            }
                        }
                    }

                    break;
            }

            NextTurn();
        }
        
        int AlgToInt(string alg) // Convert algebra notation to integer board location
        {
            int temp1 = Convert.ToInt32(alg[0]) - 97;
            int temp2 = Convert.ToInt32(alg[1]) - 48;
            return temp1 + ((temp2 - 1) * 8);
        }

        bool IsLegal(int from, int to) // Tests if a specific move is legal 
        {
            return true;
        }


        public int[] WhitePawnMove(int pos)
        {
            List<int> availablePositions = new List<int>();

            int currentColor = 0;
            if (board[pos] > 8) { currentColor = 1; }

            int file = pos % 8;
            int rank = (pos - (pos % 8)) / 8;

            if (board[pos + 8] - (board[pos + 8] % 8) - (8 * currentColor) != 0) { availablePositions.Add(pos + 8); }
            if (rank == 1) { if (board[pos + 16] - (board[pos + 16] % 8) - (8 * currentColor) != 0) { availablePositions.Add(pos + 16); } }

            int[] finalPositions = new int[availablePositions.Count];
            for (int i = 0; i < finalPositions.Length; i++) { finalPositions[i] = availablePositions[i]; }
            return finalPositions;
        }

        public int[] BlackPawnMove(int pos)
        {
            List<int> availablePositions = new List<int>();

            int currentColor = 0;
            if (board[pos] > 8) { currentColor = 1; }

            int file = pos % 8;
            int rank = (pos - (pos % 8)) / 8;

            if (board[pos - 8] - (board[pos - 8] % 8) - (8 * currentColor) != 0) { availablePositions.Add(pos + 8); }
            if (rank == 7) { if (board[pos - 16] - (board[pos - 16] % 8) - (8 * currentColor) != 0) { availablePositions.Add(pos + 16); } }

            int[] finalPositions = new int[availablePositions.Count];
            for (int i = 0; i < finalPositions.Length; i++) { finalPositions[i] = availablePositions[i]; }
            return finalPositions;
        }

        public int[] KnightMove(int pos)
        {
            List<int> availablePositions = new List<int>();

            int currentColor = 0;
            if (board[pos] > 8) { currentColor = 1; }

            int file = pos % 8;
            int rank = (pos - (pos % 8)) / 8;
            
            if (file > 0 && rank < 6) { availablePositions.Add(pos + 15); }
            if (file < 7 && rank < 6) { availablePositions.Add(pos + 17); }

            if (file < 6 && rank < 7) { availablePositions.Add(pos + 10); }
            if (file < 6 && rank > 0) { availablePositions.Add(pos - 6); }

            if (file < 7 && rank > 1) { availablePositions.Add(pos - 15); }
            if (file > 0 && rank > 1) { availablePositions.Add(pos - 17); }

            if (file > 1 && rank > 0) { availablePositions.Add(pos - 10); }
            if (file > 1 && rank < 7) { availablePositions.Add(pos + 6); }

            List<int> actualAvaiPos = new List<int>();
            foreach (int p in availablePositions) { if (board[p] - (board[p] % 8) - (8 * currentColor) != 0) { actualAvaiPos.Add(p); } }

            int[] finalPositions = new int[actualAvaiPos.Count];
            for (int i = 0; i < finalPositions.Length; i++) { finalPositions[i] = actualAvaiPos[i]; }
            return finalPositions;
        }

        public int[] BishopMove(int pos)
        {
            List<int> availablePositions = new List<int>();

            int currentColor = 0;
            if (board[pos] > 8) { currentColor = 1; }

            int currPos = pos;
            while (currPos % 8 != 0 && (currPos - (currPos % 8)) / 8 != 7)
            {
                currPos += 7;
                if (board[currPos] - (board[currPos] % 8) - (8 * currentColor) != 0) { availablePositions.Add(currPos); }
                if (board[currPos] != 0) { break; }
            } 

            currPos = pos;
            while (currPos % 8 != 0 && (currPos - (currPos % 8)) / 8 != 0)
            {
                currPos -= 9;
                if (board[currPos] - (board[currPos] % 8) - (8 * currentColor) != 0) { availablePositions.Add(currPos); }
                if (board[currPos] != 0) { break; }
            }

            currPos = pos;
            while (currPos % 8 != 7 && (currPos - (currPos % 8)) / 8 != 7)
            {
                currPos += 9;
                if (board[currPos] - (board[currPos] % 8) - (8 * currentColor) != 0) { availablePositions.Add(currPos); }
                if (board[currPos] != 0) { break; }
            }

            currPos = pos;
            while (currPos % 8 != 7 && (currPos - (currPos % 8)) / 8 != 0)
            {
                currPos -= 7;
                if (board[currPos] - (board[currPos] % 8) - (8 * currentColor) != 0) { availablePositions.Add(currPos); }
                if (board[currPos] != 0) { break; }
            }

            List<int> actualAvaiPos = new List<int>();
            foreach (int p in availablePositions) { if (board[p] - (board[p] % 8) - (8 * currentColor) != 0) { actualAvaiPos.Add(p); } }

            int[] finalPositions = new int[actualAvaiPos.Count];
            for (int i = 0; i < finalPositions.Length; i++) { finalPositions[i] = actualAvaiPos[i]; }
            return finalPositions;
        }

        public int[] RookMove(int pos)
        {
            List<int> availablePositions = new List<int>();

            int currentColor = 0;
            if (board[pos] > 8) { currentColor = 1; }

            int currPos = pos;
            while ((currPos - (currPos % 8)) / 8 != 7)
            {
                currPos += 8;
                if (board[currPos] - (board[currPos] % 8) - (8 * currentColor) != 0) { availablePositions.Add(currPos); }
                if (board[currPos] != 0) { break; }
            }

            currPos = pos;
            while ((currPos - (currPos % 8)) / 8 != 0)
            {
                currPos -= 8;
                if (board[currPos] - (board[currPos] % 8) - (8 * currentColor) != 0) { availablePositions.Add(currPos); }
                if (board[currPos] != 0) { break; }
            }

            currPos = pos;
            while (currPos % 8 != 7)
            {
                currPos += 1;
                if (board[currPos] - (board[currPos] % 8) - (8 * currentColor) != 0) { availablePositions.Add(currPos); }
                if (board[currPos] != 0) { break; }
            } 

            currPos = pos;
            while (currPos % 8 != 0)
            {
                currPos -= 1;
                if (board[currPos] - (board[currPos] % 8) - (8 * currentColor) != 0) { availablePositions.Add(currPos); }
                if (board[currPos] != 0) { break; }
            }

            List<int> actualAvaiPos = new List<int>();
            foreach (int p in availablePositions) { if (board[p] - (board[p] % 8) - (8 * currentColor) != 0) { actualAvaiPos.Add(p); } }

            int[] finalPositions = new int[actualAvaiPos.Count];
            for (int i = 0; i < finalPositions.Length; i++) { finalPositions[i] = actualAvaiPos[i]; }
            return finalPositions;
        }

        public int[] QueenMove(int pos)
        {
            List<int> availablePositions = new List<int>();

            int[] straightMoves = RookMove(pos);
            int[] diagonalMoves = BishopMove(pos);

            foreach (int move in straightMoves) { availablePositions.Add(move); }
            foreach (int move in diagonalMoves) { availablePositions.Add(move); }

            int[] finalPositions = new int[availablePositions.Count];
            for (int i = 0; i < finalPositions.Length; i++) { finalPositions[i] = availablePositions[i]; }
            return finalPositions;
        }

        public int[] KingMove(int pos)
        {
            List<int> availablePositions = new List<int>();

            int currentColor = 0;
            if (board[pos] > 8) { currentColor = 1; }

            int file = pos % 8;
            int rank = (pos - (pos % 8)) / 8;

            if (file > 0 && rank < 7) { availablePositions.Add(pos + 7); }
            if (file > 0 && rank > 0) { availablePositions.Add(pos - 9); }

            if (file < 7 && rank < 7) { availablePositions.Add(pos + 9); }
            if (file < 7 && rank > 0) { availablePositions.Add(pos - 7); }

            if (file < 7) { availablePositions.Add(pos + 1); }
            if (file > 0) { availablePositions.Add(pos - 1); }

            if (rank > 0) { availablePositions.Add(pos - 8); }
            if (rank < 7) { availablePositions.Add(pos + 8); }

            List<int> actualAvaiPos = new List<int>();
            foreach (int p in availablePositions) { if (board[p] - (board[p] % 8) - (8 * currentColor) != 0) { actualAvaiPos.Add(p); } }

            int[] finalPositions = new int[actualAvaiPos.Count];
            for (int i = 0; i < finalPositions.Length; i++) { finalPositions[i] = actualAvaiPos[i]; }
            return finalPositions;
        }


        public void CalculateThreats()
        {
            for (int i = 0; i < 64; i++)
            {
                if (board[i] == 1)
                {
                    if ((i - (i % 8)) / 8 == 6)
                    {
                        BlackThreats.Add(i - 16);
                    }
                    BlackThreats.Add(i - 8);
                }

                else if (board[i] == 2)
                {
                    foreach (int move in BishopMove(i)) { if (!BlackThreats.Contains(i)) { BlackThreats.Add(i); } }
                }

                else if (board[i] == 3)
                {
                    foreach (int move in RookMove(i)) { if (!BlackThreats.Contains(i)) { BlackThreats.Add(i); } }
                }

                else if (board[i] == 4)
                {
                    foreach (int move in QueenMove(i)) { if (!BlackThreats.Contains(i)) { BlackThreats.Add(i); } }
                }

                
            }
        }

        public void NextTurn()
        {
            if (currentTurn == 0) { currentTurn = 1; }
            else { currentTurn = 0; }
        }
    }
}
