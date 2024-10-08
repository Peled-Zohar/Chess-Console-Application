using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp
{
    internal class King : Piece
    {
            // Fields to track the maximum possible moves,whether the king has moved before and king's castling rights
            int _maxPossibleMovesNumber;
            bool firstMove;
            bool shortCastleLegal;
            bool longCastleLegal;

             // Constructor to initialize the king with name, color, row, and column
            public King(Color color, int row, int col)
                  : base(nameof(King), color, row, col)
            {
                // Setting initial values and maximum possible moves
                this._maxPossibleMovesNumber = 8;
                _allPossibleMoves = new string[_maxPossibleMovesNumber];
                ProtectedPieces = new string[_maxPossibleMovesNumber];
                firstMove = true;
                shortCastleLegal = false;
                longCastleLegal = false;
            }

            // Getter and setter for the castling field
            public bool getShortCastle()
            {
                return shortCastleLegal;
            }
            public void setShortCastle(bool shortCastleLegal)
            {
                this.shortCastleLegal = shortCastleLegal;
            }
            public bool getLongCastle()
            {
                return longCastleLegal;
            }
            public void setLongCastle(bool longCastleLegal)
            {
                this.longCastleLegal = longCastleLegal;
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


             // Method to add basic moves for the king
             public void addKingBasicMoves(Piece[,] theBoard, Piece basePiece, int kingRow, int KingCol,
                string[] possibleMovesList,int possibleMovesCount, string[] protectedPiecesList, int protectedPiecesCount) 
            {
                // running on the king grid
                for (int i = kingRow + 1; i >= kingRow - 1; i--)
                {
                    for (int j = KingCol - 1; j <= KingCol + 1; j++) 
                    {
                        if (ChessUtility.squareIsValid(i, j))
                        {
                            Piece currPiece = theBoard[i, j];
                            // piece of the same color of the king
                            if (currPiece != null && currPiece.getColor() == basePiece.getColor())   
                            {
                                AddPossibleMove(i, j, protectedPiecesList, protectedPiecesCount);
                                protectedPiecesCount++;
                            }
                            // empty cell or enemy pieces
                            else if (currPiece == null || currPiece.getColor() != basePiece.getColor()) 
                            {
                                AddPossibleMove(i, j, possibleMovesList, possibleMovesCount);
                                possibleMovesCount++;
                            }
                        }
                    }
                }
            }

             // Method to search for possible moves for the king
            public override void SearchForPossibleMoves(Piece basePiece, Piece[,] theBoard, string lastMove)
        {
            int baseRow = basePiece.getRow();
            int baseCol = basePiece.getCol();
            // Arrays to store possible moves and protected pieces
            string[] possibleMoves = new string[8];
            string[] protectedPieces = new string[8];
            int possibleMovesSize = 0, protectedCount = 0;

            // Adding basic moves for the king
            addKingBasicMoves(theBoard, basePiece, _row, _col, possibleMoves, possibleMovesSize, protectedPieces, protectedCount);
            possibleMovesSize = ChessUtility.numberOfStrings(possibleMoves);
            protectedCount = ChessUtility.numberOfStrings(protectedPieces);

            // Checking short castle validity
            if (castleShortValid(theBoard))
            {
                AddPossibleMove(_row, _col + 2, possibleMoves, possibleMovesSize);
                possibleMovesSize++;
                shortCastleLegal = true;
            }
            else
            {
                shortCastleLegal = false;
            }

            // Checking long castle validity
            if (castleLongValid(theBoard))
            {
                AddPossibleMove(_row, _col - 2, possibleMoves, possibleMovesSize);
                possibleMovesSize++;
                longCastleLegal = true;
            }
            else
            {
                longCastleLegal = false;
            }

            basePiece.setAllPossibleMove(possibleMoves);
            basePiece.ProtectedPieces = protectedPieces;
        }

            // Method to check if short castle is valid
            public bool castleShortValid(Piece[,] theBoard)
            {
                // Case when the king is already moved
                if (!firstMove) 
                    return false;
                string kingPositionStr = ChessUtility.convertSquareToString(_row, _col);
                int rookRow = _row;
                int rookCol = 7;
                Piece rook = theBoard[rookRow, rookCol];
                // Case when the relevant rook square is empty or taken by another piece 
                if (rook is not Rook) 
                    return false;
                // Case when the rook is already moved
                else if (!((Rook)rook).getFirstMove())  
                    return false;
                // Case when the route from rook to king is blocked
                else if (!((Rook)rook).ProtectedPieces.Contains(kingPositionStr)) 
                    return false;
                return true; 
            }

            // Method to check if long castle is valid
            public bool castleLongValid(Piece[,] theBoard)
            {
                // Case when the king is already moved
                 if (!firstMove)
                    return false;
                string kingPositionStr = ChessUtility.convertSquareToString(_row, _col);
                int rookRow = _row;
                int rookCol = 0;
                Piece rook = theBoard[rookRow, rookCol];
                // Case when the relevant rook square is empty or taken by another piece 
                if (rook is not Rook)
                    return false;
                // Case when the rook is already moved
                else if (!((Rook)rook).getFirstMove())
                    return false;
                // Case when the route from rook to king is blocked
                else if (!((Rook)rook).ProtectedPieces.Contains(kingPositionStr))
                    return false;
                return true;
            }
    }
}

