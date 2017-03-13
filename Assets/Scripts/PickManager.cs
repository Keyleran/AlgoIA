using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] _line1;

    [SerializeField]
    GameObject[] _line2;

    [SerializeField]
    GameObject[] _line3;

    [SerializeField]
    GameObject[] _line4;

    [SerializeField]
    Material[] _materials;

    private bool _playerTurn = true;
    private int _selectLine = 0;
    private int _selectPicks = 1;
    private int[] _nbPick = new int[4];
    private int[] _nbPickDelete = new int[4];
    private GameObject[][] _allPick;
    // Use this for initialization
    void Start ()
    {
        _nbPick[0] = _line1.Length;
        _nbPick[1] = _line2.Length;
        _nbPick[2] = _line3.Length;
        _nbPick[3] = _line4.Length;

        _allPick = new GameObject[4][];
        _allPick[0] = _line1;
        _allPick[1] = _line2;
        _allPick[2] = _line3;
        _allPick[3] = _line4;

        _nbPickDelete[0] = 0;
        _nbPickDelete[1] = 0;
        _nbPickDelete[2] = 0;
        _nbPickDelete[3] = 0;
        
        ChangeToPlayer();
    }
	
	// Update is called once per frame
	void Update ()
    {
		if(_playerTurn)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ResetHighlight();
                int temp = _selectLine;
                
                do
                {
                    _selectLine--;
                    _selectPicks = 1;
                }
                while (_selectLine > -1 && (_nbPick[_selectLine] - _nbPickDelete[_selectLine] == 0)) ;

                if (_selectLine == -1)
                {
                    _selectLine = temp;
                }

                HighlightLinePick(_selectLine);
                HighlightPicks(_selectLine, _selectPicks);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ResetHighlight();
                int temp = _selectLine;

                do
                {
                    _selectLine++;
                    _selectPicks = 1;
                }
                while (_selectLine < 4 && (_nbPick[_selectLine] - _nbPickDelete[_selectLine] == 0));

                    if (_selectLine == 4)
                {
                    _selectLine = temp;
                }

                HighlightLinePick(_selectLine);
                HighlightPicks(_selectLine, _selectPicks);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ResetHighlight();
                if (_selectPicks > 1)
                {
                    _selectPicks--;
                }

                HighlightLinePick(_selectLine);
                HighlightPicks(_selectLine, _selectPicks);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ResetHighlight();
                if (_selectPicks < _nbPick[_selectLine] - _nbPickDelete[_selectLine])
                {
                    _selectPicks++;
                }

                HighlightLinePick(_selectLine);
                HighlightPicks(_selectLine, _selectPicks);
            }

            if(Input.GetKeyDown(KeyCode.Return))
            {
                for(int i = 0; i < _selectPicks; i++)
                {
                    _allPick[_selectLine][_nbPickDelete[_selectLine] + i].SetActive(false);
                }
                _nbPickDelete[_selectLine] += _selectPicks;
                ResetHighlight();
                _playerTurn = false;
                ChangeToIA();
            }
        }
	}

    void ChangeToPlayer()
    {
        int lineId = 3;
        _selectLine = -1;
        ResetHighlight();

        while (lineId > -1)
        {
            if (_nbPick[lineId] - _nbPickDelete[lineId] > 0)
                _selectLine = lineId;
            lineId--;
        }

        if (_selectLine >= 0)
        {
            _selectPicks = 1;
            HighlightLinePick(_selectLine);
            HighlightPicks(_selectLine, _selectPicks);
            _playerTurn = true;
        }
        else
        {
            Debug.Log("Player Game Over");
        }
    }

    void ChangeToIA()
    {
        int lineId = 3;
        int nbPickToDelete = 1;
        _selectLine = -1;
        ResetHighlight();

        while (lineId > -1)
        {
            while (_nbPick[lineId] - _nbPickDelete[lineId] - nbPickToDelete >= 0)
            {
                if (CheckPickXOR(lineId, nbPickToDelete) == 0)
                {
                    _selectLine = lineId;
                    break;
                }
                else
                    nbPickToDelete++;
            }

            if (_selectLine > -1)
                break;

            lineId--;
            nbPickToDelete = 1;
        }

        if (_selectLine >= 0)
        {
            for (int i = 0; i < nbPickToDelete; i++)
            {
                _allPick[_selectLine][_nbPickDelete[_selectLine] + i].SetActive(false);
            }
            _nbPickDelete[_selectLine] += nbPickToDelete;
            ResetHighlight();
            ChangeToPlayer();
        }
        else
        {
            Debug.Log("IA Game Over");
        }
    }

    void ResetHighlight()
    {
        for(int j = 0; j < 4; j++)
        {
            for (int i = _nbPickDelete[j]; i < _nbPick[j]; i++)
            {
                _allPick[j][i].GetComponent<Renderer>().material = new Material(_materials[0]);
            }
        }
    }

    void HighlightLinePick(int lineId)
    {
        for(int i = _nbPickDelete[lineId]; i < _nbPick[lineId]; i++)
        {
            _allPick[lineId][i].GetComponent<Renderer>().material = new Material(_materials[1]);
        }
    }

    void HighlightPicks(int lineId, int nbPick)
    {
        for (int i = _nbPickDelete[lineId]; i < _nbPickDelete[lineId] + nbPick; i++)
        {
            _allPick[lineId][i].GetComponent<Renderer>().material = new Material(_materials[2]);
        }
    }

    int CheckPickXOR(int lineId, int nbPick)
    {
        int result = 0;
        for (int i = 0; i < 4; i++)
        {
            result ^= (_nbPick[i] - _nbPickDelete[i] - (i == lineId ? nbPick : 0));
        }
        return result;
    }
}
