using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp
{
    internal class Bishop : Piece
    {

        // Field to track the maximum possible moves for the bishop
        private int _maxPossibleMovesNumber;

        // Constructor to initialize the bishop with name, color, row, and column
        public Bishop(Color color, int row, int col)
            : base(nameof(Bishop), color, row, col)
        {
            // Setting initial values and maximum possible moves
            _maxPossibleMovesNumber = 13;
            _allPossibleMoves = new string[_maxPossibleMovesNumber];
            ProtectedPieces = new string[_maxPossibleMovesNumber];
        }

        // Method to find all possible moves for the bishop in the current game position
        public override void SearchForPossibleMoves(Piece basePiece, Piece[,] theBoard, string lastMove)
        {
            // Arrays to store possible moves and protected pieces
            string[] possibleMoves = new string[13];
            string[] protectedPieces = new string[13];

            // Calling move methods for diagonal directions
            basePiece.DownLeftMove(theBoard, possibleMoves, protectedPieces);
            basePiece.DownRightMove(theBoard, possibleMoves, protectedPieces);
            basePiece.UpLeftMove(theBoard, possibleMoves, protectedPieces);
            basePiece.UpRightMove(theBoard, possibleMoves, protectedPieces);

            // Setting the found moves and protected pieces
            basePiece.setAllPossibleMove(possibleMoves);
            basePiece.ProtectedPieces = protectedPieces;
        }
    }
}
