namespace ChessApp
{
    internal class Game
    {
        private const int BoardRows = 8;
        private const int BoardCols = 8;

        // Fields
        private Piece[,] _board;
        private int _gameMovesNumber;
        private int _positionsHistoryCounter;
        private Color _currentPlayer;
        private string _lastMove;
        private bool _isCheck;
        private bool _drawOffer;
        private bool _lastMoveWasPlayed;
        private StatusGame _gameStatus;
        private Piece _whiteKing;
        private Piece _blackKing;
        private string[] _positionsHistory;
        private int[] _boardsRepetitionsCounter;
        private readonly PlayerPieces _whitePieces;
        private readonly PlayerPieces _blackPieces;

        // Constructor
        public Game()
        {
            // Setting initial values to all game fields 
            _board = new Piece[BoardRows, BoardCols];
            _positionsHistory = new string[100];
            _boardsRepetitionsCounter = new int[100];
            _currentPlayer = Color.White;
            _gameStatus = StatusGame.InProgress;
            _isCheck = false;
            _drawOffer = false;
            _lastMoveWasPlayed = true;
            _lastMove = "";
            _gameMovesNumber = 0;
            _positionsHistoryCounter = 0;
            _whitePieces = new PlayerPieces(Color.White);
            _blackPieces = new PlayerPieces(Color.Black);
            _whiteKing = _whitePieces.King;
            _blackKing = _blackPieces.King;
            SettingInitialBoard();
        }

        // Setting the starting board position
        public void SettingInitialBoard()
        {
            AddPieces(_whitePieces);
            AddPieces(_blackPieces);
        }

        // Getters + Setters to fields
        public Piece[,] getBoard()
        {
            return _board;
        }

        public void setGameMovesCount(int num)
        {
            this._gameMovesNumber = num;
        }

        public int getGameMovesCount()
        {
            return this._gameMovesNumber;
        }

        public String getLastMove()
        {
            return _lastMove;
        }

        public void setLastMove(String lastMove)
        {
            this._lastMove = lastMove;
        }


        // Manage the game flow and all events from first move until the last one
        public void gamePlay()
        {
            bool playerDrawOfferStatus = false;
            Piece opponentKing;
            Color opponentPlayer;
            printBoard();
            // Updating all pieces moves for the first turn
            updateAllPiecesMoves(_lastMove);
            while (_gameStatus == StatusGame.InProgress) // The game will progress until reaching draw or mate
            {
                opponentPlayer = getOpponentPlayer(_currentPlayer);

                if (threeFoldScenario()) // Checking for draw by repetitions event
                    break;

                if (staleMateScenario(_currentPlayer)) // Checking for StaleMate event
                    break;

                // Player turn flow , from input stage until making the move on board
                playerDrawOfferStatus = playerTurn(_currentPlayer, _drawOffer, "");
                if (fiftyMovesScenario())  // Checking for 50 moves draw scenario
                    break;
                else if (deadPositionScenario()) // Checking for draw by dead position
                    break;
                else if (playerDrawOfferStatus) // Checking if draw was offered
                {
                    _lastMoveWasPlayed = false;
                    if (drawOfferScenario()) // Checking for draw by offer event 
                        break;
                }
                else
                    refuseDrawOfferScenario();
                opponentKing = getOpponentKing();
                // if in last move player gave a check
                if (checkScenario(opponentKing, opponentPlayer))
                {
                    if (mateScenario(opponentPlayer)) // Checking for CheckMate event
                        break;
                }
                else
                {
                    _isCheck = false;
                }
                printBoard();
                // switching players turns and get updated kings position
                _currentPlayer = getOpponentPlayer(_currentPlayer);
                _whiteKing = GetKingByColor(Color.White);
                _blackKing = GetKingByColor(Color.Black);

            }
        }



        // Add piece to game board
        public void AddPiece(Piece addedPiece)
        {
            _board[addedPiece.getRow(), addedPiece.getCol()] = addedPiece;
        }

        private void AddPieces(PlayerPieces playerPieces)
        {
            foreach (var piece in playerPieces.Peices)
            {
                AddPiece(piece);
            }
        }

        // Get the opponent king of current player
        public Piece getOpponentKing()
        {
            return (_currentPlayer == Color.Black) ? _whiteKing : _blackKing;
        }
        // Get the opponent player , that is not his turn
        public Color getOpponentPlayer(Color currentPlayer)
        {
            if (currentPlayer == Color.White)
                return Color.Black;
            return Color.White;
        }

        // check if the current game position is already existed 
        public int comparePositions(String currentPos, int positionsCounter)
        {
            for (int i = 0; i < positionsCounter; i++)
            {
                if (currentPos.Equals(_positionsHistory[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        // Retrieve the king position of given player from b
        public Piece GetKingByColor(Color color)
        {
            return color == Color.White
                ? _whitePieces.King
                : _blackPieces.King;
        }

        // Convert the game board to String representation for threefold draw checks
        public String boardToString()
        {
            String boardStr = "";
            boardStr += _currentPlayer;
            boardStr += " _: ";
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Piece currPiece = _board[i, j];
                    if (currPiece == null)
                        boardStr += "-";
                    else
                    {
                        boardStr += currPiece.getColor().ToString()[0];
                        boardStr += currPiece.getName().ToString()[0];
                    }
                }
            }
            boardStr += "_Castling_";
            boardStr += rightToCastle(Color.White);
            boardStr += rightToCastle(Color.Black);
            boardStr += "EnPassant_";
            boardStr += addOptionalEnPassant(Color.White);
            boardStr += addOptionalEnPassant(Color.Black);
            return boardStr;
        }


        // Check if the given Rook is already moved
        public bool isRookMovedAlready(Piece rook, Color currPlayer)
        {
            if (rook != null)
            {
                if (rook is Rook && rook.getColor() == currPlayer && ((Rook)rook).getFirstMove() == true)
                    return true;
            }
            return false;
        }


        // Return which castling moves are optional for current player in string format
        public String rightToCastle(Color currPlayer)
        {
            String castlingStr = "";
            castlingStr += currPlayer + "_";
            int rookRow = (currPlayer == Color.White) ? 0 : 7;
            Piece theKing = GetKingByColor(currPlayer);
            if (((King)theKing).getFirstMove() == false)
                return castlingStr;
            else
            {
                Piece queenSideRook = _board[rookRow, 0];
                if (isRookMovedAlready(queenSideRook, currPlayer))
                    castlingStr += "Long";
                Piece kingSideRook = _board[rookRow, 7];
                if (isRookMovedAlready(kingSideRook, currPlayer))
                    castlingStr += "Short";
            }
            return castlingStr;
        }


        // Update for each of the current pieces their possible moves except kings
        public void updatePiecesPossibleMoves(Piece[,] theBoard, String lastMove)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {

                    Piece p = theBoard[i, j];
                    if (p != null && p.getName() != "King")
                    {
                        p.SearchForPossibleMoves(p, theBoard, lastMove);
                    }
                }
            }
        }

        // Update for each of the current pieces their possible moves
        public void updateAllPiecesMoves(String lastMove)
        {
            updatePiecesPossibleMoves(_board, lastMove);
            _whiteKing.SearchForPossibleMoves(_whiteKing, _board, "");
            _blackKing.SearchForPossibleMoves(_blackKing, _board, "");
        }


        // Check if player selected a valid square - not the opponent's one or empty square 
        public bool validBaseSquare(String move)
        {
            int baseColumn = ChessUtility.getColumnFromMove(move, 0);
            int baseRow = ChessUtility.getRowFromMove(move, 1);
            Piece selectedPiece = getBoard()[baseRow, baseColumn];
            if (selectedPiece == null)
                return false;
            else if (selectedPiece.getColor() != _currentPlayer)
                return false;
            return true;
        }

        // Check if the player target square is not occupied by his pieces
        public bool validTargetSquare(String move)
        {

            int targetColumn = ChessUtility.getColumnFromMove(move, 2);
            int targetRow = ChessUtility.getRowFromMove(move, 3);
            Piece selectedPiece = getBoard()[targetRow, targetColumn];
            if (selectedPiece != null)
            {
                // Can not capture the enemy king
                if (selectedPiece.getColor() == _currentPlayer || selectedPiece.getName() == "King")
                    return false;
            }
            return true;
        }

        // Check whether player typed the same square twice(as base cell and target cell)
        public bool isUserTypeTheSameSquare(String userInput)
        {
            if (userInput[0] == userInput[2] && userInput[1] == userInput[3])
                return false;
            return true;
        }

        // Get the requested move from user
        public String getUserInput()
        {
            String userMsg = (_drawOffer == true) ? " player type 'draw' to accept the offer, " +
            "or any legal move to refuse" : " player please enter a move or offer a draw :";
            Console.WriteLine(_currentPlayer + userMsg);
            String userInput = Console.ReadLine();
            return userInput;
        }

        // Check if user asked or agreed for draw instead of typing regular chess move
        public bool userInputIsDraw(String userMove)
        {
            if (userMove != "draw" && userMove != "DRAW")
                return false;
            return true;
        }

        // Check if all input characters are valid 
        public bool isAllMoveCharactersValid(String userMove)
        {
            if (!ChessUtility.validFileNumber(userMove[0]) || !ChessUtility.validFileNumber(userMove[2]) ||
                    !ChessUtility.validRankNumber(userMove[1]) || !ChessUtility.validRankNumber(userMove[3]))
                return false;
            return true;
        }


        // Ensure the user entered a valid move format 
        public String validMoveFormat()
        {
            bool validMove = false;
            String userMove = "";
            String userInput = "";
            do
            {
                userInput = getUserInput();
                if (ChessUtility.isUserInputEmpty(userInput))
                    continue;
                userMove = userInput.Trim();
                if (!ChessUtility.isMoveLengthValid(userMove))
                    Console.WriteLine("The input must be 4 characters length. Please try again");
                else if (userInputIsDraw(userMove))
                {
                    return userMove;
                }
                else if (!isAllMoveCharactersValid(userMove))
                {
                    Console.WriteLine("The input contains illegal characters. Please try again");
                }
                else if (!isUserTypeTheSameSquare(userMove))
                {
                    Console.WriteLine("The input contains the same cell twice. Please try again");
                }
                else
                    validMove = true;
            } while (!validMove);
            return userMove;
        }


        // Draw functions

        // Add position to boards history if not exists, if current board repeated already twice return true
        public bool isThreeFoldDrawOccurred()
        {
            String currentPosition = boardToString();
            int comparisonIndex = comparePositions(currentPosition, _positionsHistoryCounter);
            if (comparisonIndex != -1)
            {
                _boardsRepetitionsCounter[comparisonIndex]++;
                if (_boardsRepetitionsCounter[comparisonIndex] == 2)
                    return true;
            }
            else
            {
                _positionsHistory[_positionsHistoryCounter] = currentPosition;
                _positionsHistoryCounter++;
            }
            return false;
        }


        // Handle a draw event with proper message
        public void drawGameEvents(String typeOfDraw)
        {
            _gameStatus = StatusGame.Draw;
            if (typeOfDraw == "ThreeFold")
                Console.WriteLine("The game is draw , draw by repetition");
            else if (typeOfDraw == "Stalemate")
                Console.WriteLine("The game is draw , stalemate position");
            else if (typeOfDraw == "Dead position")
            {
                printBoard();
                Console.WriteLine("The game is draw , Dead position case");
            }
            else if (typeOfDraw == "Offer") // draw by offer
            {
                printBoard();
                Console.WriteLine("The game is a draw , draw offer accepted by both players");
            }

            else
            {
                printBoard();
                Console.WriteLine("The game is draw , 50 moves played without any pawn move or capturing");
            }
            Console.WriteLine("The game result is : 1/2 - 1/2");
        }


        // Check if in the last turn a draw was offered or answered
        public bool drawOfferScenario()
        {
            if (_drawOffer)
            {
                drawGameEvents("Offer");
                return true;
            }
            else
            {
                Console.WriteLine("Draw offer sent by " + _currentPlayer + " player");
                _drawOffer = true;
            }
            return false;
        }


        // Check if player refused to draw offer
        public void refuseDrawOfferScenario()
        {
            if (_drawOffer)
            {
                Console.WriteLine("Draw offer refused by " + _currentPlayer + " player. The turn is" +
                    " now going back to the second player");
                _lastMoveWasPlayed = false;
            }
            else
                _lastMoveWasPlayed = true;
            _drawOffer = false;
        }

        // Check if the current player is in stalemate  
        public bool staleMateScenario(Color currentPlayer)
        {
            if (_isCheck == false)
            {
                if (playerHasAnyLegalMove(_board, currentPlayer))
                {
                    drawGameEvents("Stalemate");
                    return true;
                }
            }
            return false;
        }

        // Check if the 50 moves rule was reached
        public bool fiftyMovesScenario()
        {
            if (_gameMovesNumber == 100)
            {
                drawGameEvents("50");
                return true;
            }
            return false;
        }

        // Check if a game ended with dead position(no side can win)
        public bool deadPositionScenario()
        {
            if (deadPositionDraw("King") || deadPositionDraw("Bishop") || deadPositionDraw("Night"))
            {
                drawGameEvents("Dead position");
                return true;
            }
            return false;
        }


        // In case threefold event has reached, will pass it to the draws manager function
        public bool threeFoldScenario()
        {
            // Checking the current board position only when a actual move was done,ignoring draw offers.
            if (_lastMoveWasPlayed)
            {
                if (isThreeFoldDrawOccurred())
                {
                    drawGameEvents("ThreeFold");
                    return true;
                }
            }
            return false;
        }


        // Check if current board is in insufficient material is reached
        public bool deadPositionDraw(String pieceName)
        {
            int whitePieces = 0, blackPieces = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Piece currPiece = getBoard()[i, j];
                    if (currPiece != null)
                    {
                        if (currPiece.getName() != pieceName && currPiece.getName() != "King")
                            return false;
                        else
                        {
                            if (currPiece.getName() == pieceName)
                            {
                                if (currPiece.getColor() == Color.White)
                                    whitePieces++;
                                else
                                    blackPieces++;
                            }
                        }
                    }
                }
            }

            if (whitePieces != 1 || blackPieces != 1)
                return false;
            return true;
        }


        // Update if player is in check event
        public bool checkScenario(Piece opponentKing, Color opponentPlayer)
        {
            if (searchForCheck(_board, opponentKing))
            {
                Console.WriteLine(opponentPlayer + " player is in check");
                _isCheck = true;
                return true;
            }
            return false;
        }

        // Check if player is in check
        public bool searchForCheck(Piece[,] theBoard, Piece king)
        {
            String kingLocation = ChessUtility.convertSquareToString(king.getRow(), king.getCol());
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Piece p = getBoard()[i, j];
                    if (p != null && p.getColor() != king.getColor())
                    {
                        if (p.getName() == "Pawn")
                        {
                            if (((Pawn)p).getCaptureMoves().Contains(kingLocation))
                            {
                                return true;
                            }
                        }
                        else if (p.getAllPossibleMoves().Contains(kingLocation))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        // Update if player got checkmate
        public bool mateScenario(Color opponentPlayer)
        {
            if (playerHasAnyLegalMove(_board, opponentPlayer))
            {
                _gameStatus = StatusGame.Win;
                printBoard();
                Console.WriteLine("Checkmate! " + _currentPlayer + " player you are the winner");
                printIntegerGameResult(_currentPlayer);
                return true;
            }
            return false;
        }

        // Print the game result in numeric version - 1:0 or 0:1
        public void printIntegerGameResult(Color gameWinner)
        {
            Console.WriteLine("Game result is - " + (gameWinner == Color.White ? "1:0" : "0:1"));
        }

        // Check if player has any legal move based on current board and check event
        public bool playerHasAnyLegalMove(Piece[,] theBoard, Color currPlayer)
        {
            for (int i = 0; i < 8; i++) // iterate over the board looking for opponent pieces
            {
                for (int j = 0; j < 8; j++)
                {

                    Piece currPiece = theBoard[i, j];
                    if (currPiece != null && currPiece.getColor() == currPlayer)
                    {
                        for (int movesIndex = 0; movesIndex < currPiece.getAllPossibleMoves().Length; movesIndex++)
                        { // running on all piece possible moves
                            String targetMoveString = currPiece.getAllPossibleMoves()[movesIndex];
                            if (targetMoveString == null || targetMoveString == "")
                                break;
                            if (currPiece is King)
                            {
                                int kingMoveColumn = targetMoveString[1] - '0';
                                // castle move is not relevant
                                if (currPiece.getCol() - kingMoveColumn == 2 || currPiece.getCol() - kingMoveColumn == -2)
                                    continue;
                            }
                            if (isSimulateMoveValid(targetMoveString, currPiece))
                                return false;
                        }
                    }
                }
            }
            return true;
        }


        // In case of piece capturing or pawn move , we do not need to store all the previous boards
        public void resetBoardsHistory()
        {
            _positionsHistory = new string[100];
            _boardsRepetitionsCounter = new int[100];
            _positionsHistoryCounter = 0;
        }

        // Handle pawn promotion event with current player
        public void promotionDecision(Piece pawn)
        {
            bool validChoice;
            string input;
            do
            {
                validChoice = true;
                Console.WriteLine("Please choose which piece you want to promote to. Type 'Queen' for queen,\n" +
                    "'Rook' for rook, 'Knight' for knight, or 'Bishop' for bishop");
                input = Console.ReadLine();
                switch (input)
                {
                    case "Queen":
                        {
                            AddPiece(new Queen(pawn.getColor(), pawn.getRow(), pawn.getCol()));
                            break;
                        }
                    case "Rook":
                        {
                            AddPiece(new Rook(pawn.getColor(), pawn.getRow(), pawn.getCol()));
                            break;
                        }
                    case "Knight":
                        {
                            AddPiece(new Knight(pawn.getColor(), pawn.getRow(), pawn.getCol()));
                            break;
                        }
                    case "Bishop":
                        {
                            AddPiece(new Bishop(pawn.getColor(), pawn.getRow(), pawn.getCol()));
                            break;
                        }
                    default:
                        {
                            validChoice = false;
                            break;
                        }
                }
            } while (!validChoice);
            int pieceRow = pawn.getRow() + 1;
            int pieceCol = pawn.getCol();
            int baseRow = _currentPlayer == Color.White ? pieceRow - 1 : pieceRow + 1;
            string promotionMoveStr = "";
            promotionMoveStr += baseRow;
            promotionMoveStr += pieceCol;
            promotionMoveStr += pieceRow;
            promotionMoveStr += pieceCol;
            updateAllPiecesMoves(promotionMoveStr);
        }

        // Check if last move was a pawn promotion
        public void promotion(Piece pawn)
        {
            if ((pawn.getColor() == Color.White && pawn.getRow() == 7) ||
            (pawn.getColor() == Color.Black && pawn.getRow() == 0))
            {
                promotionDecision(pawn);
            }
        }

        // Add all the available en Passant moves for current player in string format
        public String addOptionalEnPassant(Color currPlayer)
        {
            String enPassantStr = "";
            int enPassantRow, direction;
            String lastTurn = "";
            enPassantRow = (_currentPlayer == Color.White) ? 4 : 3;
            direction = (_currentPlayer == Color.White) ? 1 : -1;
            Piece targetPiece = null;
            enPassantStr += currPlayer + "_";
            for (int i = 0; i < 8; i++)
            {
                Piece currPiece = _board[enPassantRow, i];
                // optional en a passant capture,check if it is valid
                if (currPiece is Pawn && currPiece.getColor() == currPlayer)
                {
                    if (((Pawn)currPiece).getEnPassantRight()) // check if right capture is valid
                    {
                        enPassantStr += optionsToMakeEnPassant("Right", currPiece, targetPiece,
                            enPassantStr, enPassantRow, lastTurn, direction, i);
                        lastTurn = "";
                    }
                    else if (((Pawn)currPiece).getEnPassantLeft()) // check if left capture is valid
                    {
                        enPassantStr += optionsToMakeEnPassant("Left", currPiece, targetPiece, enPassantStr,
                            enPassantRow, lastTurn, direction, i);
                    }
                }
            }
            return enPassantStr;
        }


        // Convert optional enPassant move to string format if it valid
        public String optionsToMakeEnPassant(String enPassantSide, Piece pawn, Piece targetPiece,
            String enPassantStr, int enPassantRow, String lastTurn, int moveDirection
           , int baseColumn)
        {
            // adding one to the base column when capturing to the right
            int captureDirection = (enPassantSide == "Right") ? 1 : -1;
            targetPiece = _board[enPassantRow, baseColumn + captureDirection];
            lastTurn += ChessUtility.convertNumToFile(baseColumn);
            lastTurn += enPassantRow;
            lastTurn += ChessUtility.convertNumToFile(baseColumn + captureDirection);
            lastTurn += enPassantRow + moveDirection;
            if (simulateEnPassant(pawn, targetPiece, enPassantRow + moveDirection, baseColumn + captureDirection, enPassantRow, baseColumn, lastTurn))
            {
                // add the file name to the string
                enPassantStr += ChessUtility.convertNumToFile(baseColumn) + "_";
                enPassantStr += "Right";
                pawn.setRow(enPassantRow);
                pawn.setCol(baseColumn);
                AddPiece(pawn);
                _board[enPassantRow + moveDirection, baseColumn + captureDirection] = null;
                _board[enPassantRow, baseColumn + captureDirection] = targetPiece;
                updateAllPiecesMoves(_lastMove);
            }
            return enPassantStr;
        }


        // Check whether enPassant move is allowed on board(handling check or not breaking any pin)
        public bool enPassantIsAllowed(Piece pieceToMove, Piece targetPiece, int baseRow, int baseColumn,
            int targetRow, int targetColumn, String currentMove)
        {
            // enPassant to the right
            if ((targetColumn > pieceToMove.getCol()) && ((Pawn)pieceToMove).getEnPassantRight())
            {
                if (simulateEnPassant(pieceToMove, targetPiece, targetRow, targetColumn, baseRow, baseColumn, currentMove))
                {
                    return true;
                }
                return false;
            }

            // enPassant to the left
            else if ((targetColumn < pieceToMove.getCol()) && ((Pawn)pieceToMove).getEnPassantLeft())
            {

                if (simulateEnPassant(pieceToMove, targetPiece, targetRow, targetColumn, baseRow, baseColumn, currentMove))
                {
                    return true;
                }
            }
            return false;
        }


        // Simulate a requested enPassant move on current board and passing it validity to relevant function 
        public bool simulateEnPassant(Piece basePiece, Piece targetPiece, int targetRow, int targetCol, int baseRow,
               int baseCol, String currMove)
        {
            Piece opponentPawn = getBoard()[baseRow, targetCol];
            makeSimulateMove(basePiece, null, baseRow, baseCol, targetRow, targetCol, true);
            updateAllPiecesMoves(currMove);
            bool foundCheck = searchForCheck(_board, GetKingByColor(basePiece.getColor()));
            if (foundCheck) // this move is not dealing with the check
            {
                makeSimulateMoveBack(basePiece, null, targetRow, targetCol, baseRow, baseCol, opponentPawn);
                //makeSimulateMove(basePiece, null, targetRow, targetCol, baseRow, baseCol, true);
                //getBoard()[targetRow, targetCol] = null;
                updateAllPiecesMoves(_lastMove); // update the board pieces with the current possible moves
                return false;
            }
            return true;
        }



        // Return true if the simulate move dealt with current check position,otherwise return false
        public bool isSimulateMoveValid(String targetMove, Piece basePiece)
        {
            int targetRow = targetMove[0] - '0';
            int targetCol = targetMove[1] - '0';
            int baseRow = basePiece.getRow();
            int baseCol = basePiece.getCol();
            Piece opponentPawn = null; // In case of enPassant move
            Piece targetPiece = getBoard()[targetRow, targetCol];
            bool askForEnPassant = false;
            String moveStr = "";
            if (basePiece is Pawn)
            {
                // en Passant move
                if (targetPiece == null && baseCol != targetCol)
                {
                    askForEnPassant = true;
                    opponentPawn = getBoard()[baseRow, targetCol];
                }

            }
            makeSimulateMove(basePiece, null, baseRow, baseCol, targetRow, targetCol, askForEnPassant);
            moveStr += ChessUtility.convertNumToFile(baseCol);
            moveStr += (char)(baseRow + '0');
            moveStr += ChessUtility.convertNumToFile(targetCol);
            moveStr += (char)(targetRow + '0');
            updateAllPiecesMoves(moveStr);
            bool foundCheck = searchForCheck(_board, GetKingByColor(basePiece.getColor()));
            makeSimulateMoveBack(basePiece, targetPiece, targetRow, targetCol, baseRow, baseCol, opponentPawn);

            /*
            makeSimulateMove(basePiece, targetPiece, targetRow, targetCol, baseRow, baseCol, askForEnPassant);
            if(askForEnPassant)
            {
                board[baseRow, targetCol] = opponentPawn;
            }
            */
            updateAllPiecesMoves(_lastMove);
            if (foundCheck)
                return false;
            return true;
        }


        // Make a simulate move on game board , for further calculation
        public void makeSimulateMove(Piece thePiece, Piece targetPiece, int pieceRow, int pieceColumn, int targetRow,
           int targetColumn, bool askForEnPassent)
        {
            thePiece.setRow(targetRow);
            thePiece.setCol(targetColumn);
            AddPiece(thePiece);
            getBoard()[pieceRow, pieceColumn] = targetPiece;
            if (askForEnPassent)
                getBoard()[pieceRow, targetColumn] = targetPiece;
        }


        // Take back a simulate move on game board , for specific cases
        public void makeSimulateMoveBack(Piece thePiece, Piece targetPiece, int pieceRow, int pieceColumn, int targetRow,
           int targetColumn, Piece opponentPawn)
        {

            thePiece.setRow(targetRow);
            thePiece.setCol(targetColumn);
            AddPiece(thePiece);
            getBoard()[pieceRow, pieceColumn] = targetPiece;
            if (opponentPawn != null) // the tested move is enPassant
            {
                getBoard()[targetRow, pieceColumn] = opponentPawn;

            }
        }

        // Check if a simulate move by the king is valid
        public bool simulateKingMove(Piece basePiece, int targetRow, int targetCol, int baseRow,
           int baseCol, int currntKingCol, String currMove)
        {
            makeSimulateMove(basePiece, null, baseRow, currntKingCol, targetRow, targetCol, false);
            getBoard()[baseRow, currntKingCol] = null;
            updateAllPiecesMoves(currMove);
            bool foundCheck = searchForCheck(_board, GetKingByColor(basePiece.getColor()));
            if (foundCheck == true)
            {
                // undo the previous move
                makeSimulateMove(basePiece, null, targetRow, targetCol, baseRow, baseCol, false);
                updateAllPiecesMoves(_lastMove);
                return false;
            }
            return true;
        }


        // Check whether a simple move(not enPassant or castling) is allowed using simulated technique
        public bool regularMoveIsAllowed(Piece pieceToMove, int baseRow, int baseColumn, int targetRow
                   , int targetColumn, String currentMove)
        {
            makeSimulateMove(pieceToMove, null, baseRow, baseColumn, targetRow, targetColumn, false);
            updateAllPiecesMoves(currentMove);
            bool foundCheck = searchForCheck(_board, GetKingByColor(pieceToMove.getColor()));
            if (foundCheck == true)
                return false;
            return true;
        }

        // Check if a castling move is allowed for player
        public bool castlingIsAllowed(Piece pieceToMove, int baseRow, int baseColumn, int kingCol, String currentMove,
               bool kingFirstMove)
        {
            if (!simulateKingMove(pieceToMove, baseRow, kingCol, baseRow, baseColumn,
                pieceToMove.getCol(), currentMove))
            {
                ((King)pieceToMove).setFirstMove(kingFirstMove);
                return false;
            }
            return true;
        }

        // Update rook position and properties after successful castling event
        public void moveRookForCastling(int baseRow, int targetColumn, bool isShortCastling)
        {
            int rookBaseColumnDifference = (isShortCastling == true) ? 1 : -2;
            int rookTargetColOffset = (isShortCastling == true) ? -1 : 1;
            Piece rook = getBoard()[baseRow, targetColumn + rookBaseColumnDifference];
            rook.setCol(targetColumn + rookTargetColOffset);
            AddPiece(rook);
            getBoard()[baseRow, targetColumn + rookBaseColumnDifference] = null;
            ((Rook)rook).setFirstMove(false);
        }


        // Check if player request for castling is standing in game rules - from checks perspective
        public bool castlingMoveScenario(Piece pieceToMove, int baseRow, int baseColumn, int targetRow
            , int targetColumn, int columnDifference, String currentMove, bool kingFirstMove)
        {
            bool CheckStatus = searchForCheck(_board, pieceToMove);

            if (CheckStatus)
            {
                ((King)pieceToMove).setFirstMove(kingFirstMove);
                return false; // The king is in check , castling is not valid
            }
            else if (columnDifference == -2) // Short castle case
            {

                // Check all squares king has to pass for castling and verify none of them
                //is threatened by opponent pieces(following the game rules)
                for (int kingCol = baseColumn + 1; kingCol <= targetColumn; kingCol++)
                {
                    if (!castlingIsAllowed(pieceToMove, baseRow, baseColumn, kingCol, currentMove, kingFirstMove))
                        return false;
                }
                // the short castle is valid , updating rook position as well
                moveRookForCastling(baseRow, targetColumn, true);
                return true;
            }
            else
            {
                // long castle case
                for (int kingCol = baseColumn - 1; kingCol >= targetColumn; kingCol--)
                {
                    if (!castlingIsAllowed(pieceToMove, baseRow, baseColumn, kingCol,
                        currentMove, kingFirstMove))
                        return false;
                }
                moveRookForCastling(baseRow, targetColumn, false);
                return true;
            }
        }

        // Setting to relevant pieces their firstMove property as it was before simulate move
        public void resetPiecesFirstMove(Piece pieceToMove, bool pawnStatus, bool kingStatus, bool rookStatus)
        {
            if (pieceToMove is Pawn)
                ((Pawn)pieceToMove).setStartSquare(pawnStatus);
            else if (pieceToMove is King)
                ((King)pieceToMove).setFirstMove(kingStatus);
            else if (pieceToMove is Rook)
                ((Rook)pieceToMove).setFirstMove(rookStatus);
        }

        // Printing the current game board
        public void printBoard()
        {
            printBoardFrame(BoardRows, 'A');
            Console.WriteLine("  -----------------------------------------");
            printPieces();
            printBoardFrame(BoardRows, 'A');
        }

        // Helper function for printing the board and the pieces
        public void printPieces()
        {
            for (int i = BoardRows - 1; i >= 0; i--)
            {
                Console.Write((i + 1) + " ");
                for (int j = 0; j < BoardRows; j++)
                {
                    String pos = "";
                    Console.Write("| ");
                    if (_board[i, j] == null)
                    {
                        Console.Write("   ");
                        continue;
                    }
                    Piece currPiece = _board[i, j];
                    if (currPiece != null)
                    {
                        pos += currPiece.getColor().ToString()[0];
                        pos += currPiece.getName().ToString()[0];
                    }
                    Console.Write(pos + " ");
                }
                Console.Write("| ");
                Console.Write((i + 1) + " ");
                Console.WriteLine("");
                Console.WriteLine("  -----------------------------------------");
            }
        }

        // Printing the board files naming(A - H)
        public void printBoardFrame(int rows, char tav)
        {
            Console.Write("  ");
            for (int i = rows; i >= 0; i--)
            {
                if (i == rows)
                {
                    Console.Write($"{"",-2}");
                    continue;
                }
                else
                {
                    Console.Write($"{tav,-5}");
                    tav++;
                }
            }
            Console.WriteLine("");
        }


        // Get valid input from player or draw response
        public bool playerTurn(Color currPlayer, bool drawOffer, String str)
        {
            bool userInput = false;
            String move = "";
            while (!userInput)
            {
                //check the format of the move - 4 characters with the pattern of [A-H],[1-8],[A-H],[1-8]
                move = validMoveFormat();
                if (move == "draw" || move == "DRAW")
                {
                    return true;
                }

                else if (drawOffer)  // player did not agree to draw offer
                    return false;
                if (!validBaseSquare(move))
                    Console.WriteLine("The base square is either empty or occupied by the opponent. Please try again");

                // check if player selected one of his pieces and not the opponent's one
                else if (!validTargetSquare(move))

                {
                    Console.WriteLine("The target square is already occupied by your piece or by the enemy king." +
                        " Please try again");
                }
                else if (!pieceMoveIsAllowed(move, _isCheck)) // handling check scenarios and pinned moves
                {
                    if (_isCheck)
                        Console.WriteLine("You have to respond to the check. Please try again");
                    else
                        Console.WriteLine("The selected piece cannot reach the target square. Please try again");
                }
                else
                    userInput = true;
            }
            _lastMove = move;
            return false;
        }


        // Check if moves counter and boards list should reset after a specific moves
        public void setGameStatusAfterMove(Piece pieceToMove, Piece targetPiece)
        {
            // Pawn move resetting the 50 moves rule by definition
            if (pieceToMove is Pawn)
            {
                setGameMovesCount(0);
                resetBoardsHistory();
                promotion(pieceToMove);
            }
            // Capturing move resetting the 50 moves rule by definition
            else if (targetPiece != null)
            {
                setGameMovesCount(0);
                resetBoardsHistory();
            }
            else
                _gameMovesNumber++;
        }


        // Check if given move is allowed for the player , according piece movement
        // board status , check scenarios and more ...
        public bool pieceMoveIsAllowed(string move, bool isCheck)
        {

            // Getting the base square and target square out of player move
            int baseRow = ChessUtility.getRowFromMove(move, 1);
            int baseCol = ChessUtility.getColumnFromMove(move, 0);
            int targetRow = ChessUtility.getRowFromMove(move, 3);
            int targetCol = ChessUtility.getColumnFromMove(move, 2);
            String targetPositionStr = ChessUtility.convertSquareToString(targetRow, targetCol);
            bool kingFirstMove = false, rookFirstMove = false, pawnMove = false;
            Piece targetPiece = getBoard()[targetRow, targetCol];
            Piece pieceToMove = getBoard()[baseRow, baseCol];
            // Making sure the move is part of piece optional moves(moves list contains it)
            if (pieceToMove.getAllPossibleMoves().Contains(targetPositionStr))
            {
                if (pieceToMove is Pawn) // Pawn move 
                {
                    pawnMove = ((Pawn)pieceToMove).getStartSquare();
                    ((Pawn)pieceToMove).setStartSquare(false);
                    if (targetPiece == null && (targetCol != baseCol)) // if asks for en a passant
                    {
                        // Checking that enPassant is allowed
                        if (enPassantIsAllowed(pieceToMove, targetPiece, baseRow, baseCol, targetRow, targetCol, move))
                        {
                            setGameMovesCount(0);
                            resetBoardsHistory();
                            return true;
                        }
                        else
                            return false;
                    }
                }
                else if (pieceToMove is King) // King move
                {
                    kingFirstMove = ((King)pieceToMove).getFirstMove();
                    ((King)pieceToMove).setFirstMove(false);
                    int columnDifference = pieceToMove.getCol() - targetCol;
                    // asking for one of the castling options
                    if (columnDifference == 2 || columnDifference == -2)
                    {
                        // Checking whether castling is allowed
                        if (castlingMoveScenario(pieceToMove, baseRow, baseCol, targetRow, targetCol,
                            columnDifference, move, kingFirstMove))
                        {
                            updateAllPiecesMoves(move);
                            _gameMovesNumber++;
                            return true;
                        }
                        else
                            return false;
                    }
                }
                else if (pieceToMove is Rook) // Rook move
                {
                    rookFirstMove = ((Rook)pieceToMove).getFirstMove();
                    ((Rook)pieceToMove).setFirstMove(false);
                }
                // Checking all the other regular moves validity , by simulate move technique
                if (!regularMoveIsAllowed(pieceToMove, baseRow, baseCol, targetRow, targetCol, move))
                {
                    // Making the simulate move back , since requested move is not allowed
                    makeSimulateMove(pieceToMove, targetPiece, targetRow, targetCol, baseRow, baseCol, false);
                    resetPiecesFirstMove(pieceToMove, pawnMove, kingFirstMove, rookFirstMove);
                    // update the board pieces with the current possible moves after simulate move
                    updateAllPiecesMoves(_lastMove);
                    return false;
                }
                // Simulated move is valid and saved in the board for the next turn
                else
                {
                    // Check if any game field need an update
                    setGameStatusAfterMove(pieceToMove, targetPiece);
                    return true;
                }
            }
            return false;
        }



        private class PlayerPieces
        {
            public PlayerPieces(Color color)
            {
                Color = color;
                var firstRow = color == Color.White ? 0 : 7;
                var secondRow = color == Color.White ? 1 : 6;


                King = new King(color, firstRow, 4);
                Queen = new Queen(color, firstRow, 3);
                Rooks = [
                    new Rook(color, firstRow, 0),
                    new Rook(color, firstRow, 7)
                ];
                Knights = [
                    new Knight(color, firstRow, 1),
                    new Knight(color, firstRow, 6)
                ];
                Bishops = [
                    new Bishop(color, firstRow, 2),
                    new Bishop(color, firstRow, 5)
                ];

                // I could have specify them like I did the other arrays, but linq is shorter.
                Pawns = Enumerable.Range(0, 8)
                    .Select(i => new Pawn(color, secondRow, i))
                    .ToArray();

                Peices = Rooks
                    .Cast<Piece>()
                    .Concat(Knights)
                    .Concat(Bishops)
                    .Concat(Pawns)
                    .Append(King)
                    .Append(Queen)
                    .ToArray();
            }

            public Color Color { get; }
            public King King { get; }
            public Queen Queen { get; }
            public Knight[] Knights { get; }
            public Rook[] Rooks { get; }
            public Bishop[] Bishops { get; }
            public Pawn[] Pawns { get; }

            public Piece[] Peices { get; }
        }
    }
}
