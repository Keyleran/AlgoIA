using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Domineering : MonoBehaviour
{
    public enum Mode
    {
        MinMax,
        MinMaxAlphaBeta,
        Negamax,
        NegamaxTransposition
    };

    struct Transpostion
    {
        int[] bestMove;
        int depth;
        int score;
        bool minorant;
        uint hashcode;
    }

    [SerializeField]
    int _width = 5;

    [SerializeField]
    int _heigth = 5;

    [SerializeField]
    Material[] _materials;

    [SerializeField]
    bool _iaStart = false;

    [SerializeField]
    Mode _mode = Mode.MinMax;

    [SerializeField]
    bool _killerMove = false;

    [SerializeField]
    bool _nullMove = false;

    [SerializeField]
    int _recursivity = 5;
    
    private GameObject[][] _quadrillage;
    private bool _playerTurn = true;
    private bool[][] _quadrillageState;

    private int _posCursorX = 0;
    private int _posCursorY = 0;

    private int[][] _killMove;

    private uint _stateZobrist = 0;
    private uint[] _stateBoard;

    List<Transpostion> _tableTransposition;

    // Use this for initialization
    void Start ()
    {
        bool pairWidth = _width % 2 == 0 ? true : false;
        bool pairHeigth = _heigth % 2 == 0 ? true : false;

        float posX = (-_width / 2) + (pairWidth  ? 0.5f : 0);
        float posY = (_heigth / 2) - (pairHeigth ? 0.5f : 0);

        _quadrillage = new GameObject[_heigth][];
        _quadrillageState = new bool[_heigth][];
        for (int i = 0; i < _heigth; i++)
        {
            _quadrillage[i] = new GameObject[_width];
            _quadrillageState[i] = new bool[_width];
            for (int j = 0; j < _width; j++)
            {
                _quadrillage[i][j] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _quadrillage[i][j].transform.position = new Vector3(posX, posY, 0);
                _quadrillageState[i][j] = false;
                posX++;
            }
            posX = (-_width / 2) + (pairWidth ? 0.5f : 0);
            posY--;
        }

        _killMove = new int[_recursivity][];
        for (int i = 0; i < _recursivity; i++)
            _killMove[i] = new int[2] { -1, -1 };

        _stateBoard = new uint[_recursivity];
        for (int i = 0; i < _recursivity; i++)
            _stateBoard[i] = (uint) Random.Range(0, sizeof(uint));

        if (_iaStart)
            ChangeToIA();
        else
            ChangeToPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerTurn)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ResetHighlight();

                int lineY = _posCursorY - 1;
                int colX = -1;
                while (lineY > -1)
                {
                    colX = GetPosXFree(lineY);
                    if (colX == -1)
                        lineY--;
                    else
                    {
                        _posCursorX = colX;
                        _posCursorY = lineY;
                        break;
                    }
                }
                HighlightPlayerPick(_posCursorX, _posCursorY);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ResetHighlight();

                int lineY = _posCursorY + 1;
                int colX = -1;
                while (lineY < _heigth)
                {
                    colX = GetPosXFree(lineY);
                    if (colX == -1)
                        lineY++;
                    else
                    {
                        _posCursorX = colX;
                        _posCursorY = lineY;
                        break;
                    }
                }
                HighlightPlayerPick(_posCursorX, _posCursorY);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ResetHighlight();
                int posX = GetNextPosXFree(_posCursorY, _posCursorX, -1);

                if (posX != -1)
                    _posCursorX = posX;
                HighlightPlayerPick(_posCursorX, _posCursorY);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ResetHighlight();
                int posX = GetNextPosXFree(_posCursorY, _posCursorX, 1);

                if (posX != -1)
                    _posCursorX = posX;
                HighlightPlayerPick(_posCursorX, _posCursorY);
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                SetPlayerPick(_posCursorX, _posCursorY);
                _playerTurn = false;
                ChangeToIA();
            }
        }
    }

    void ChangeToPlayer()
    {
        _posCursorX = -1;
        _posCursorY = -1;
        ResetHighlight();

        for (int lineY = 0; lineY < _heigth; lineY++)
        {
            for (int colX = 0; colX < _width - 1; colX++)
            {
                if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false)
                {
                    _posCursorX = colX;
                    _posCursorY = lineY;
                }

                if (_posCursorX >= 0 && _posCursorY >= 0)
                    break;
            }

            if (_posCursorX >= 0 && _posCursorY >= 0)
                break;
        }

        if (_posCursorX >= 0 && _posCursorY >= 0)
        {
            HighlightPlayerPick(_posCursorX, _posCursorY);
            _playerTurn = true;
        }
        else
        {
            Debug.Log(NbSolucePlayer());
            Debug.Log("Player Game Over");
        }
    }


    void ChangeToIA()
    {
        _posCursorX = -1;
        _posCursorY = -1;

        switch(_mode)
        {
            case Mode.MinMax:
                int heuristic = -5000;

                for (int lineY = 0; lineY < _heigth - 1; lineY++)
                {
                    for (int colX = 0; colX < _width; colX++)
                    {
                        if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY + 1][colX] == false)
                        {
                            _quadrillageState[lineY][colX] = true;
                            _quadrillageState[lineY + 1][colX] = true;

                            int tempHeur = NbSoluceIA() - NbSolucePlayer();
                            if (tempHeur > heuristic)
                            {
                                heuristic = tempHeur;
                                _posCursorX = colX;
                                _posCursorY = lineY;
                            }

                            _quadrillageState[lineY][colX] = false;
                            _quadrillageState[lineY + 1][colX] = false;
                        }
                    }
                }
                break;
            case Mode.MinMaxAlphaBeta:
                Max(_recursivity, -5000, 5000);
                break;
            case Mode.Negamax:
                NegaMax(_recursivity, -NbSolucePlayer() - 50, NbSoluceIA() + 50);
                break;
            case Mode.NegamaxTransposition:
                NegaMaxTranspostion(_recursivity, -NbSolucePlayer() - 50, NbSoluceIA() + 50);
                break;

        }

        if (_posCursorX >= 0 && _posCursorY >= 0)
        {
            SetIAPick(_posCursorX, _posCursorY);
            ChangeToPlayer();
        }
        else
        {
            Debug.Log(NbSolucePlayer());
            Debug.Log(NbSoluceIA());
            Debug.Log("IA Game Over");
        }
    }

    void ResetHighlight()
    {
        for (int j = 0; j < _heigth; j++)
        {
            for (int i = 0; i < _width; i++)
            {
                if(_quadrillageState[j][i] == false)
                    _quadrillage[j][i].GetComponent<Renderer>().material = new Material(_materials[0]);
            }
        }
    }

    void HighlightPlayerPick(int x, int y)
    {
        _quadrillage[y][x].GetComponent<Renderer>().material = new Material(_materials[1]);
        _quadrillage[y][x + 1].GetComponent<Renderer>().material = new Material(_materials[1]);
    }

    void SetPlayerPick(int x, int y)
    {
        _quadrillage[y][x].GetComponent<Renderer>().material = new Material(_materials[3]);
        _quadrillage[y][x + 1].GetComponent<Renderer>().material = new Material(_materials[3]);

        _quadrillageState[y][x] = true;
        _quadrillageState[y][x + 1] = true;

        _stateZobrist = _stateZobrist ^ _stateBoard[x + (y * _width)] ^ _stateBoard[x + 1 + (y * _width)];
    }

    void SetIAPick(int x, int y)
    {
        _quadrillage[y][x].GetComponent<Renderer>().material = new Material(_materials[2]);
        _quadrillage[y + 1][x].GetComponent<Renderer>().material = new Material(_materials[2]);

        _quadrillageState[y][x] = true;
        _quadrillageState[y + 1][x] = true;

        _stateZobrist = _stateZobrist ^ _stateBoard[x + (y * _width)] ^ _stateBoard[x + ((y + 1) * _width)];
    }


    int GetPosXFree(int lineY)
    {
        int posX = -1;
        
        for(int colX = 0; colX < _width - 1; colX++)
        {
            if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false)
            {
                posX = colX;
            }

            if (posX >= 0)
                break;
        }

        return posX;
    }

    int GetNextPosXFree(int lineY, int colX, int offset)
    {
        int posX = -1;

        colX += offset;
        while ((offset == 1 && colX < _width - 1) || (offset == -1 && colX > -1))
        {
            if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false)
            {
                posX = colX;
            }

            if (posX >= 0)
                break;

            colX += offset;
        }

        return posX;
    }


    int NbSolucePlayer()
    {
        int nbSoluce = 0;

        for (int lineY = 0; lineY < _heigth; lineY++)
        {
            for (int colX = 0; colX < _width - 1; colX++)
            {
                if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false)
                {
                    nbSoluce++;
                }
            }
        }
        return nbSoluce;
    }

    int NbSoluceIA()
    {
        int nbSoluce = 0;

        for (int lineY = 0; lineY < _heigth - 1; lineY++)
        {
            for (int colX = 0; colX < _width; colX++)
            {
                if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY + 1][colX] == false)
                {
                    nbSoluce++;
                }
            }
        }
        return nbSoluce;
    }

    int Min(int depth, int alpha, int beta)
    {
        if (depth == 0 || (NbSoluceIA() == 0 && NbSolucePlayer() == 0))
            return NbSoluceIA() - NbSolucePlayer();

        for (int lineY = 0; lineY < _heigth; lineY++)
        {
            for (int colX = 0; colX < _width - 1; colX++)
            {
                if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false)
                {
                    _quadrillageState[lineY][colX] = true;
                    _quadrillageState[lineY][colX + 1] = true;

                    int tempHeur = Max(depth - 1, alpha, beta);

                    _quadrillageState[lineY][colX] = false;
                    _quadrillageState[lineY][colX + 1] = false;

                    if (tempHeur < beta)
                    {
                        beta = tempHeur;
                        if (alpha > beta)
                            return alpha;
                    }
                }
            }
        }

        return beta;
    }

    int Max(int depth, int alpha, int beta)
    {
        if (depth == 0 || (NbSoluceIA() == 0 && NbSolucePlayer() == 0))
            return NbSoluceIA() - NbSolucePlayer();

        for (int lineY = 0; lineY < _heigth - 1; lineY++)
        {
            for (int colX = 0; colX < _width; colX++)
            {
                if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY + 1][colX] == false)
                {
                    _quadrillageState[lineY][colX] = true;
                    _quadrillageState[lineY + 1][colX] = true;

                    int tempHeur = Min(depth - 1, alpha, beta);

                    _quadrillageState[lineY][colX] = false;
                    _quadrillageState[lineY + 1][colX] = false;

                    if (tempHeur > alpha)
                    {
                        alpha = tempHeur;
                        if (alpha > beta)
                            return beta;

                        if (depth == _recursivity)
                        {
                            _posCursorX = colX;
                            _posCursorY = lineY;
                        }
                    }
                }
            }
        }

        return alpha;
    }

    int NegaMax(int depth, int alpha, int beta)
    {
        bool turnIA = ((_recursivity - depth) % 2 == 0);
        if (depth == 0 || (turnIA && NbSoluceIA() == 0) || (!turnIA && NbSolucePlayer() == 0))
            return NbSoluceIA() - NbSolucePlayer();

        // Get KillerMove
        List<int[]> moves = getMovesAvalaible(turnIA);

        #region Kill Move
        if (_killerMove && _killMove[_recursivity - depth][0] != -1 && _killMove[_recursivity - depth][1] != -1)
        {
            if ((turnIA  && _quadrillageState[_killMove[_recursivity - depth][0]][_killMove[_recursivity - depth][1]] == false && _quadrillageState[_killMove[_recursivity - depth][0] + 1][_killMove[_recursivity - depth][1]] == false) ||
                (!turnIA && _quadrillageState[_killMove[_recursivity - depth][0]][_killMove[_recursivity - depth][1]] == false && _quadrillageState[_killMove[_recursivity - depth][0]][_killMove[_recursivity - depth][1] + 1] == false))
            {
                int[] kill = new int[2] { _killMove[_recursivity - depth][0], _killMove[_recursivity - depth][1] };
                moves.Insert(0, kill);
                _killMove[_recursivity - depth][0] = -1;
                _killMove[_recursivity - depth][1] = -1;
            }
        }
        #endregion

        #region Null Move
        if (_nullMove && depth - 3 > 0)
        {
            int[] worstMove = getWorstMoveAvalaible(turnIA);

            if(worstMove[0] != -1 && worstMove[1] != -1)
            {
                int lineY = worstMove[0];
                int colX  = worstMove[1];

                _quadrillageState[lineY][colX] = true;
                _quadrillageState[lineY + (turnIA ? 1 : 0)][colX + (turnIA ? 0 : 1)] = true;

                int tempHeur = -NegaMax(depth - 3, -beta, -alpha);

                _quadrillageState[lineY][colX] = false;
                _quadrillageState[lineY + (turnIA ? 1 : 0)][colX + (turnIA ? 0 : 1)] = false;
                
                if (alpha >= beta)
                {
                    return beta;
                }
            }
        }
        #endregion


        for (int i = 0; i < moves.Count; i++)
        {
            int lineY = moves[i][0];
            int colX  = moves[i][1];

            _quadrillageState[lineY][colX] = true;
            _quadrillageState[lineY + (turnIA ? 1 : 0)][colX + (turnIA ? 0 : 1)] = true;

            int tempHeur = -NegaMax(depth - 1, -beta, -alpha);



            _quadrillageState[lineY][colX] = false;
            _quadrillageState[lineY + (turnIA ? 1 : 0)][colX + (turnIA ? 0 : 1)] = false;

            if (tempHeur > alpha)
            {
                alpha = tempHeur;

                if (alpha >= beta)
                {
                    if(_killerMove)
                        _killMove[_recursivity - depth] = moves[i];

                    return beta;
                }

                if (depth == _recursivity)
                {
                    _posCursorX = colX;
                    _posCursorY = lineY;
                }
            }
        }

        return alpha;
    }

    List<int[]> getMovesAvalaible(bool turnIA)
    {
        List<int[]> moves = new List<int[]>();

        for (int lineY = 0; lineY < _heigth - (turnIA ? 1 : 0); lineY++)
        {
            for (int colX = 0; colX < _width - (turnIA ? 0 : 1); colX++)
            {
                if ((turnIA && _quadrillageState[lineY][colX] == false && _quadrillageState[lineY + 1][colX] == false) ||
                    (!turnIA && _quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false))
                {
                    int[] pos = new int[2] { lineY, colX };
                    moves.Add(pos);
                }
            }
        }

        return moves;
    }

    int[] getWorstMoveAvalaible(bool turnIA)
    {
        int[] moves = new int[2] { -1, -1 };

        int worstCoup = turnIA ? 500 : -500;
        for (int lineY = 0; lineY < _heigth - (turnIA ? 1 : 0); lineY++)
        {
            for (int colX = 0; colX < _width - (turnIA ? 0 : 1); colX++)
            {
                if ((turnIA && _quadrillageState[lineY][colX] == false && _quadrillageState[lineY + 1][colX] == false) ||
                    (!turnIA && _quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false))
                {
                    _quadrillageState[lineY][colX] = true;
                    _quadrillageState[lineY + (turnIA ? 1 : 0)][colX + (turnIA ? 0 : 1)] = true;
                    
                    int soluce = turnIA ? NbSoluceIA() : NbSolucePlayer();
                    if ((turnIA && worstCoup > soluce) || (!turnIA && worstCoup < soluce))
                    {
                        worstCoup = soluce;
                        moves[0] = lineY;
                        moves[1] = colX;
                    }

                    _quadrillageState[lineY][colX] = false;
                    _quadrillageState[lineY + (turnIA ? 1 : 0)][colX + (turnIA ? 0 : 1)] = false;
                }
            }
        }

        return moves;
    }


    int NegaMaxTranspostion(int depth, int alpha, int beta)
    {
        bool turnIA = ((_recursivity - depth) % 2 == 0);
        if (depth == 0 || (turnIA && NbSoluceIA() == 0) || (!turnIA && NbSolucePlayer() == 0))
            return NbSoluceIA() - NbSolucePlayer();
        
        List<int[]> moves = getMovesAvalaible(turnIA);
        
        for (int i = 0; i < moves.Count; i++)
        {
            int lineY = moves[i][0];
            int colX = moves[i][1];

            _quadrillageState[lineY][colX] = true;
            _quadrillageState[lineY + (turnIA ? 1 : 0)][colX + (turnIA ? 0 : 1)] = true;

            int tempHeur = -NegaMaxTranspostion(depth - 1, -beta, -alpha);



            _quadrillageState[lineY][colX] = false;
            _quadrillageState[lineY + (turnIA ? 1 : 0)][colX + (turnIA ? 0 : 1)] = false;

            if (tempHeur > alpha)
            {
                alpha = tempHeur;

                if (alpha >= beta)
                {
                    if (_killerMove)
                        _killMove[_recursivity - depth] = moves[i];

                    return beta;
                }

                if (depth == _recursivity)
                {
                    _posCursorX = colX;
                    _posCursorY = lineY;
                }
            }
        }

        return alpha;
    }


}
