using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PositionController : MonoBehaviour
{
    private const int totalPieces = 6;
    private const int chessBoardWidth = 8;
    private const int chessBoardHeight = 8;

    public GameObject[] blackGameObj = new GameObject[totalPieces];
    public GameObject[] whiteGameObj = new GameObject[totalPieces];
    public GameObject chessBoard;
    public Camera chessCamera;
    private Transform chessCamTransform;

    private enum Pieces
    {
        Pawn = 0,
        Knight,
        Bishop,
        Rook,
        Queen,
        King,
    }

    private enum PieceColor
    {
        white = 0,
        black,
    }

    private Dictionary<Pieces, GameObject> blackPieces = new Dictionary<Pieces, GameObject>();
    private Dictionary<Pieces, GameObject> whitePieces = new Dictionary<Pieces, GameObject>();

    private const float yOffset = -0.00540F;

    private List<Vector3> availableSquares = new List<Vector3>();
    private List<GameObject> piecesOnBoard = new List<GameObject>();

    private class PieceInfo
    {
        public Pieces piece;
        public PieceColor color;
        public Vector3 square;

        public PieceInfo(Pieces piece, PieceColor color, Vector3 square)
        {
            this.piece = piece;
            this.color = color;
            this.square = square;
        }
    }

    private List<PieceInfo> boardPosition = new List<PieceInfo>();
    //private List<int[]> boardPositionFormatted = new List<int[]>();
    private List<int[][]> boardPositionFormatted = new List<int[][]>();
    private bool isDataExported = false;
    private int imageCount = 0;
    public int totalImagesCount = 5;
    public int imageWidth, imageHeight;

    private int frameCount = 0;
    private int frameDelay = 50;

    private string singlePieceName;
    private List<Vector3> cameraPositions = new List<Vector3>();
    public int totalCameraPositions;

    private List<(PieceColor, Pieces)> allPieces = new List<(PieceColor, Pieces)>();
    private int currentPieceIndex = 0;
    private float pieceYOffset = 0;

    private enum State
    {
        Idle,
        Init,
        Generating,
        Done,
    }

    private State state;

    // Start is called before the first frame update
    void Start()
    {
        chessCamTransform = chessCamera.GetComponent<Transform>();

        InitializePiecesDict();
        PopulateAllPieces();
        PopulateCameraPositions(totalCameraPositions);

        state = State.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        //OperateGenerateImage();
        OperateSinglePiece();
    }

    private void InitializePiecesDict()
    {
        for (int i = 0; i < totalPieces; i++)
        {
            blackPieces.Add((Pieces)i, blackGameObj[i]);
        }

        for (int i = 0; i < totalPieces; i++)
        {
            whitePieces.Add((Pieces)i, whiteGameObj[i]);
        }
    }

    private void PopulateAllPieces()
    {
        foreach (PieceColor color in Enum.GetValues(typeof(PieceColor)))
        {
            foreach (Pieces piece in Enum.GetValues(typeof(Pieces)))
            {
                allPieces.Add((color, piece));
            }
        }
    }

    private void resetAvailableSquares()
    {
        availableSquares.Clear();
        for (int x = 0; x < chessBoardWidth; x++)
        {
            for (int z = 0; z < chessBoardHeight; z++)
            {
                availableSquares.Add(new Vector3(z, yOffset, x));
            }
        }
    }

    private void ClearBoardPieces()
    {
        foreach (GameObject piece in piecesOnBoard)
        {
            Destroy(piece);
        }

        piecesOnBoard.Clear();
    }

    private void ClearBoardPosition()
    {
        boardPosition.Clear();
    }

    private void ResetBoard()
    {
        resetAvailableSquares();
        ClearBoardPieces();
        ClearBoardPosition();
    }

    private Vector3 GetRandomSquare()
    {
        int index = UnityEngine.Random.Range(0, availableSquares.Count);
        Vector3 square = availableSquares[index];
        availableSquares.RemoveAt(index);
        return square;
    }

    private Vector3 GetRandomSquare2()
    {
        int index = 0;
        Vector3 square = availableSquares[index];
        availableSquares.RemoveAt(index);
        return square;
    }

    private void SpawnPiece(Pieces piece, PieceColor color)
    {
        Vector3 location = GetRandomSquare();
        SpawnPiece(piece, color, location);
    }

    private void SpawnPiece(Pieces piece, PieceColor color, Vector3 location)
    {
        GameObject pieceObj = null;

        switch (color)
        {
            case PieceColor.white:
                pieceObj = Instantiate(whitePieces[piece], location, Quaternion.identity, chessBoard.transform);
                break;
            case PieceColor.black:
                pieceObj = Instantiate(blackPieces[piece], location, Quaternion.identity, chessBoard.transform);
                break;
            default:
                break;
        }
        piecesOnBoard.Add(pieceObj);
        pieceYOffset = pieceObj.GetComponent<MeshFilter>().mesh.bounds.size.y;

        PieceInfo pieceInfo = new PieceInfo(piece, color, location);
        boardPosition.Add(pieceInfo);
    }

    private void SpawnBlackAndWhitePawn()
    {
        ResetBoard();

        int whitePawns = UnityEngine.Random.Range(0, 33);
        int blackPawns = UnityEngine.Random.Range(0, 33);

        for (int i = 0; i < whitePawns; i++)
        {
            
            SpawnPiece(Pieces.Pawn, PieceColor.white);
        }

        for (int i = 0; i < blackPawns; i++)
        {
            SpawnPiece(Pieces.Pawn, PieceColor.black);
        }
    }

    private void GenerateImages(int count, int width, int height)
    {
        SpawnBlackAndWhitePawn();
        FormatBoardPosition();
        string imageName = "board_" + count.ToString();
        ScreenshotHandler.TakeScreenshot(width, height, imageName);
    }

    private void OperateGenerateImage()
    {
        if (frameCount < frameDelay)
        {
            frameCount++;
        }
        else
        {
            if (imageCount < totalImagesCount)
            {
                GenerateImages(imageCount, imageWidth, imageHeight);
                imageCount++;

                if (imageCount % 1000 == 0)
                {
                    Debug.Log("Count: " + imageCount.ToString());
                }
            }
            else if (!isDataExported)
            {
                Debug.Log("Exporting board pos");
                ExportFormattedBoardPosition();
                isDataExported = true;
            }
        }
    }

    private void PopulateCameraPositions(int num)
    {
        //hard coded, TODO?
        float xzStart = -0.5F;
        float xzEnd = -xzStart;
        float dxz = (xzEnd - xzStart) / (num-1);

        float yStart = 2.0F;
        float yEnd = 3.0F;
        float dy = (yEnd - yStart) / (num-1);

        for (float x = xzStart; x <= xzEnd; x += dxz)
        {
            for (float z = xzStart; z <= xzEnd; z += dxz)
            {
                for (float y = yStart; y <= yEnd; y += dy)
                {
                    cameraPositions.Add(new Vector3(x, y, z));
                }
            }     
        }
    }
    private void SetupSinglePiece(PieceColor color, Pieces piece)
    {
        chessCamTransform.position = new Vector3(0, 0, 0);

        if (color == PieceColor.white)
        {
            singlePieceName = "white_";
        }
        else
        {
            singlePieceName = "black_";
        }

        switch (piece)
        {
            case Pieces.Pawn:
                singlePieceName += "pawn_";
                SpawnPiece(piece, color, new Vector3(0, yOffset, 0));
                break;

            case Pieces.Knight:
                singlePieceName += "knight_";
                SpawnPiece(piece, color, new Vector3(0, yOffset, 0));
                break;
            case Pieces.Bishop:
                singlePieceName += "bishop_";
                SpawnPiece(piece, color, new Vector3(0, yOffset, 0));
                break;
            case Pieces.Rook:
                singlePieceName += "rook_";
                SpawnPiece(piece, color, new Vector3(0, yOffset, 0));
                break;
            case Pieces.Queen:
                singlePieceName += "queen_";
                SpawnPiece(piece, color, new Vector3(0, yOffset, 0));
                break;
            case Pieces.King:
                singlePieceName += "king_";
                SpawnPiece(piece, color, new Vector3(0, yOffset, 0));
                break;
            default:
                break;
        }
    }

    private void SinglePiece(int count, int width, int height)
    {
        if (count >= cameraPositions.Count)
        {
            imageCount = totalImagesCount;
        }
        else
        {
            //set up camera
            chessCamTransform.position = cameraPositions[count];
            chessCamTransform.LookAt(piecesOnBoard[0].transform.position + new Vector3(0, pieceYOffset * 20.0F, 0));

            //randomize board pos, maybe also if board incldued or not
            //rand(0,1); if 1: board.y = -1//aka hide, white background
            //else board.pos = vec3(0,0,0) + vec3(rand(-1,1))

            string imageName = singlePieceName + count.ToString();
            ScreenshotHandler.TakeScreenshot(width, height, imageName);
        }
    }

    private void OperateSinglePiece()
    {
        if (frameCount < frameDelay)
        {
            frameCount++;
        }
        else
        {
            if (state == State.Idle)
            {
                (PieceColor color, Pieces piece) = allPieces[currentPieceIndex];
                SetupSinglePiece(color, piece);
                state = State.Generating;
            }
            if (state == State.Generating)
            {
                if (imageCount < totalImagesCount)
                {
                    SinglePiece(imageCount, imageWidth, imageHeight);
                    imageCount++;

                    if (imageCount % 100 == 0)
                    {
                        Debug.Log("Count: " + imageCount.ToString());
                    }
                }
                else
                {
                    currentPieceIndex++;
                    if (currentPieceIndex >= allPieces.Count)
                    {
                        Debug.Log("All Done");
                        state = State.Done;
                    }
                    else
                    {
                        ResetBoard();
                        frameCount = 0;
                        imageCount = 0;
                        state = State.Idle;
                    }
                    
                }
            }
            if (state == State.Done)
            {
                ;
            }
        }
    }

    private void FormatBoardPosition()
    {
        int[][] position = new int[2][]; //TODO should be totalPieces * 2 with all peices

        for (int i = 0; i < 2; i++)//TODO should be totalPieces * 2 with all peices
        {
            position[i] = new int[chessBoardHeight * chessBoardWidth];
        }

        int linearIndex;
        int pieceIndex;

        foreach (PieceInfo pieceInfo in boardPosition)
        {
            linearIndex = (int)((pieceInfo.square.z * chessBoardWidth) + pieceInfo.square.x);
            pieceIndex = (int)pieceInfo.piece;

            if (pieceInfo.color == PieceColor.black)
            {
                pieceIndex += 1; // totalPieces; TODO DONT FORGET
            }

            position[pieceIndex][linearIndex] = 1;
        }

        boardPositionFormatted.Add(position);
    }

    private void FormatBoardPosition_old()
    {
        int[] outputArr = new int[chessBoardWidth * chessBoardHeight];
        int linearIndex;
        int pieceClass;

        foreach (PieceInfo pieceInfo in boardPosition)
        {
            linearIndex = (int)((pieceInfo.square.z * chessBoardWidth) + pieceInfo.square.x);
            pieceClass = (int)pieceInfo.piece + 1;

            if (pieceInfo.color == PieceColor.black)
            {
                pieceClass += 1; // totalPieces; TODO DONT FORGET
            }

            outputArr[linearIndex] = pieceClass;
        }

        //boardPositionFormatted.Add(outputArr);
    }

    private void ExportFormattedBoardPosition()
    {
        string fileName = Application.dataPath + "/../Images/board_position.csv";
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
        {
            foreach (int[][] arr2 in boardPositionFormatted)
            {
                foreach (int[] arr in arr2)
                {
                    file.Write(string.Join(",", arr));
                    file.Write(Environment.NewLine);
                }
            }
        }
    }
}
