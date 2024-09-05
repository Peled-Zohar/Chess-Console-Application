using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp
{
    public enum Color
    {
        Black,
        White
    }

    internal abstract class Piece
    {
        // Fields
        protected string _name;
        protected Color _color;
        protected int _row;
        protected int _col;
        protected string[] _allPossibleMoves;
        protected int _possibleMovesCount;

        // Constructor to initialize the piece with name, color, row, and column
        protected Piece(string name, Color color, int row, int col)
        {
            _name = name;
            _color = color;
            _row = row;
            _col = col;
            _allPossibleMoves = [];
            _possibleMovesCount = 0;
            ProtectedPieces = [];
        }

        #region properties 

        public string[] ProtectedPieces { get; set; }

        public void setAllPossibleMove(string[] moves) => _allPossibleMoves = moves;

        public int getPossibleMovesCount() => _possibleMovesCount;

        public void setPossibleMovesCount(int possibleMovesCount) => _possibleMovesCount = possibleMovesCount;

        public string[] getAllPossibleMoves() => _allPossibleMoves;

        public string getName() => _name;

        public Color getColor() => _color;

        public int getRow() => _row;

        public int getCol() => _col;

        public void setRow(int row) => _row = row;

        public void setCol(int col) => _col = col;

        #endregion properties 

        // Virtual function , that each of derived classes will override as needed
        public abstract void SearchForPossibleMoves(Piece basePiece, Piece[,] theBoard, string lastMove);

        // Adding a square to the piece moves list based on it status
        public bool AddSquareToPossibleMoves(Piece[,] board, int squareRow, int squareColumn,
            string[] moves, int movesNumber, string[] protectedSquares, int protectedSquaresCount)
        {
            Piece currPiece = board[squareRow, squareColumn];
            // If the square is empty add move to list
            if (currPiece == null)
            {
                AddPossibleMove(squareRow, squareColumn, moves, movesNumber);

            }
            // If square is occupied by an opponent's piece, add it to list and stop checking this direction
            else if (currPiece._color != _color)
            {
                AddPossibleMove(squareRow, squareColumn, moves, movesNumber);
                return false;
            }
            else
            {
                // If the square is occupied by a piece of the same color, add it to protected squares
                AddPossibleMove(squareRow, squareColumn, protectedSquares, protectedSquaresCount);
                return false;
            }
            return true;
        }

        // Checks and adds optional rightward moves for the current chess piece
        public void RightMove(Piece[,] board, string[] moves, string[] protectedSquares) => Move(board, moves, protectedSquares, Direction.Right);
        // Checks and adds optional leftward moves for the current chess piece
        public void LeftMove(Piece[,] board, string[] moves, string[] protectedSquares) => Move(board, moves, protectedSquares, Direction.Left);
        // Checks and adds optional upward moves for the current chess piece
        public void UpMove(Piece[,] board, string[] moves, string[] protectedSquares) => Move(board, moves, protectedSquares, Direction.Up);

        // Checks and adds optional downward moves for the current chess piece
        public void DownMove(Piece[,] board, string[] moves, string[] protectedSquares) => Move(board, moves, protectedSquares, Direction.Down);

        // Checks and adds optional top-right diagonal moves for the current chess piece
        public void UpRightMove(Piece[,] board, string[] moves, string[] protectedSquares) => Move(board, moves, protectedSquares, Direction.UpRight);

        // Checks and adds optional top-left diagonal moves for the current chess piece
        public void UpLeftMove(Piece[,] board, string[] moves, string[] protectedSquares) => Move(board, moves, protectedSquares, Direction.UpLeft);

        // Checks and adds optional down-left diagonal moves for the current chess piece
        public void DownLeftMove(Piece[,] board, string[] moves, string[] protectedSquares) => Move(board, moves, protectedSquares, Direction.DownLeft);

        // Checks and adds optional down-right diagonal moves for the current chess piece
        public void DownRightMove(Piece[,] board, string[] moves, string[] protectedSquares) => Move(board, moves, protectedSquares, Direction.DownRight);

        private void Move(Piece[,] board, string[] moves, string[] protectedSquares, Direction direction)
        {
            int currentRow = _row + direction.RowIncrement;
            int currentColumn = _col + direction.ColumnIncrement;
            int movesNumber = ChessUtility.numOfCurrentMoves(moves);
            int protectedSquaresCount = ChessUtility.numOfCurrentMoves(protectedSquares);

            // I don't mind checking all of them even if they don't change because it simplifies the code significantly
            while (currentRow >= 0 && currentRow < 8 && currentColumn >= 0 && currentColumn < 8)
            {
                if (AddSquareToPossibleMoves(board, currentRow, currentColumn, moves, movesNumber, protectedSquares, protectedSquaresCount))
                    movesNumber++;
                else
                    break;
                currentRow += direction.RowIncrement;
                currentColumn += direction.ColumnIncrement;
            }
        }

        private class Direction
        {
            public static readonly Direction Up = new(1, 0);
            public static readonly Direction Down = new(-1, 0);
            public static readonly Direction Left = new(0, -1);
            public static readonly Direction Right = new(0, 1);
            public static readonly Direction UpLeft = new(1, -1);
            public static readonly Direction UpRight = new(1, 1);
            public static readonly Direction DownLeft = new(-1, -1);
            public static readonly Direction DownRight = new(-1, 1);

            private Direction(int rowIncrement, int columnIncrement)
            {
                RowIncrement = rowIncrement;
                ColumnIncrement = columnIncrement;
            }

            public int RowIncrement { get; }
            public int ColumnIncrement { get; }
        }

        // Convert a square position to string representation and add it to piece list
        public void AddPossibleMove(int currRow, int currCol, string[] moves, int count)
        {
            string pos = "";
            char row = (char)(currRow + 48);
            char col = (char)(currCol + 48);
            pos += row;
            pos += col;
            moves[count] = pos;
        }
    }
}

