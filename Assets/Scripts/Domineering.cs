using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Domineering : MonoBehaviour
{
    [SerializeField]
    int _width = 5;

    [SerializeField]
    int _heigth = 5;

    [SerializeField]
    Material[] _materials;

    [SerializeField]
    bool _minimax = true;

    [SerializeField]
    int _recursivity = 5;

    private GameObject[][] _quadrillage;
    private bool _playerTurn = true;
    private bool[][] _quadrillageState;

    private int _posCursorX = 0;
    private int _posCursorY = 0;

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
                _posCursorX = 0;

                int lineY = _posCursorY - 1;
                int colX = 0;
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
                _posCursorX = 0;

                int lineY = _posCursorY + 1;
                int colX = 0;
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

        int lineY = 0;
        while (lineY < _heigth)
        {
            int colX = 0;
            while (colX < _width - 1)
            {
                if(_quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false)
                {
                    _posCursorX = colX;
                    _posCursorY = lineY;
                }

                if (_posCursorX >= 0 && _posCursorY >= 0)
                    break;

                colX++;
            }

            if (_posCursorX >= 0 && _posCursorY >= 0)
                break;

            lineY++;
        }

        if (_posCursorX >= 0 && _posCursorY >= 0)
        {
            HighlightPlayerPick(_posCursorX, _posCursorY);
            _playerTurn = true;
        }
        else
        {
            Debug.Log("Player Game Over");
        }
    }


    void ChangeToIA()
    {
        _posCursorX = -1;
        _posCursorY = -1;

        if(!_minimax)
        {
            int heuristic = -100;

            int lineY = 0;
            while (lineY < _heigth - 1)
            {
                int colX = 0;
                while (colX < _width)
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
                    colX++;
                }
                lineY++;
            }
        }
        else
        {
            MiniMax(_recursivity, 0);
        }

        if (_posCursorX >= 0 && _posCursorY >= 0)
        {
            SetIAPick(_posCursorX, _posCursorY);
            ChangeToPlayer();
        }
        else
        {
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
    }

    void SetIAPick(int x, int y)
    {
        _quadrillage[y][x].GetComponent<Renderer>().material = new Material(_materials[2]);
        _quadrillage[y + 1][x].GetComponent<Renderer>().material = new Material(_materials[2]);

        _quadrillageState[y][x] = true;
        _quadrillageState[y + 1][x] = true;
    }


    int GetPosXFree(int lineY)
    {
        int posX = -1;

        int colX = 0;
        while (colX < _width - 1)
        {
            if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false)
            {
                posX = colX;
            }

            if (posX >= 0)
                break;

            colX++;
        }

        return posX;
    }

    int GetNextPosXFree(int lineY, int colX, int offset)
    {
        int posX = -1;

        colX += offset;
        while ((offset == 1 && colX < _width - 1) || (offset == -1 && colX > - 1))
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

        int lineY = 0;
        while (lineY < _heigth)
        {
            int colX = 0;
            while (colX < _width - 1)
            {
                if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false)
                {
                    nbSoluce++;
                }
                colX++;
            }
            lineY++;
        }
        return nbSoluce;
    }

    int NbSoluceIA()
    {
        int nbSoluce = 0;

        int lineY = 0;
        while (lineY < _heigth - 1)
        {
            int colX = 0;
            while (colX < _width)
            {
                if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY + 1][colX] == false)
                {
                    nbSoluce++;
                }
                colX++;
            }
            lineY++;
        }
        return nbSoluce;
    }

    int MiniMax(int depth, int nbDepth)
    {
        bool iaTurn = nbDepth % 2 == 0 ? true : false;

        if(depth == 0)
            return NbSoluceIA() - NbSolucePlayer();

        int finalHeur = 0;
        if(iaTurn)
        {
            // Max
            int heuristic = -100;

            int lineY = 0;
            while (lineY < _heigth - 1)
            {
                int colX = 0;
                while (colX < _width)
                {
                    if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY + 1][colX] == false)
                    {
                        _quadrillageState[lineY][colX] = true;
                        _quadrillageState[lineY + 1][colX] = true;

                        int tempHeur = MiniMax(depth - 1, nbDepth + 1);
                        if (tempHeur > heuristic)
                        {
                            heuristic = tempHeur;
                            finalHeur = tempHeur;

                            if(nbDepth == 0)
                            {
                                _posCursorX = colX;
                                _posCursorY = lineY;
                            }
                        }

                        _quadrillageState[lineY][colX] = false;
                        _quadrillageState[lineY + 1][colX] = false;
                    }
                    colX++;
                }
                lineY++;
            }
        }
        else
        {
            // Min
            int heuristic = 100;

            int lineY = 0;
            while (lineY < _heigth)
            {
                int colX = 0;
                while (colX < _width - 1)
                {
                    if (_quadrillageState[lineY][colX] == false && _quadrillageState[lineY][colX + 1] == false)
                    {
                        _quadrillageState[lineY][colX] = true;
                        _quadrillageState[lineY][colX + 1] = true;

                        int tempHeur = MiniMax(depth - 1, nbDepth + 1);
                        if (tempHeur < heuristic)
                        {
                            heuristic = tempHeur;
                            finalHeur = tempHeur;
                        }

                        _quadrillageState[lineY][colX] = false;
                        _quadrillageState[lineY][colX + 1] = false;
                    }
                    colX++;
                }
                lineY++;
            }
        }
        return finalHeur;
    }
}
