using ChessApp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp
{
    internal class Queen : Piece
    {
        // Field to track the maximum possible moves for the queen
        int maxPossibleMovesNumber;

        // Constructor to initialize the queen with name, color, row, and column
        public Queen(Color color, int row, int col)
                    : base(nameof(Queen), color, row, col)
        {
            // Setting initial values and maximum possible moves
            this.maxPossibleMovesNumber = 27;
            _allPossibleMoves = new string[maxPossibleMovesNumber];
            ProtectedPieces = new string[maxPossibleMovesNumber];
        }


        // Method to find all possible moves for the queen in the current game position
        public override void SearchForPossibleMoves(Piece basePiece, Piece[,] theBoard, String lastMove)
        {
            // Arrays to store possible moves and protected pieces
            string[] possibleMoves = new string[27];
            string[] protectedPieces = new string[27];

            // Calling move methods for all directions
            basePiece.RightMove(theBoard, possibleMoves, protectedPieces);
            basePiece.LeftMove(theBoard, possibleMoves, protectedPieces);
            basePiece.UpMove(theBoard, possibleMoves, protectedPieces);
            basePiece.DownMove(theBoard, possibleMoves, protectedPieces);
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
