using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp
{
    internal class Rook : Piece
    {
        // Fields to track the maximum possible moves and whether the rook has moved before
        int maxPossibleMovesNumber;
        bool firstMove;


        // Constructor to initialize the rook with name, color, row, and column
        public Rook(Color color, int row, int col)
            : base(nameof(Rook), color, row, col)
        {
            // Setting initial values and maximum possible moves
            this.maxPossibleMovesNumber = 14;
            _allPossibleMoves = new string[maxPossibleMovesNumber];
            ProtectedPieces = new String[maxPossibleMovesNumber];
            firstMove = true;
        }


        // Getter and setter for the firstMove field
        public bool getFirstMove()
        {
            return firstMove;
        }
        public void setFirstMove(bool move)
        {
            this.firstMove = move;
        }



        // Method to find all possible moves for the rook in the current game position
        public override void SearchForPossibleMoves(Piece basePiece, Piece[,] theBoard, string lastMove)
        {
            // Arrays to store possible moves and protected pieces
            string[] possibleMoves = new string[14];
            string[] protectedPieces = new string[14];

            // Calling move methods to find possible moves in all directions
            basePiece.RightMove(theBoard, possibleMoves, protectedPieces);
            basePiece.LeftMove(theBoard, possibleMoves, protectedPieces);
            basePiece.UpMove(theBoard, possibleMoves, protectedPieces);
            basePiece.DownMove(theBoard, possibleMoves, protectedPieces);

            // Setting the found moves and protected pieces
            basePiece.setAllPossibleMove(possibleMoves);
            basePiece.ProtectedPieces = protectedPieces;
        }
    }

}
