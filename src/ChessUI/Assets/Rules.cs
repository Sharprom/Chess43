using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ChessRules;

public class Rules : MonoBehaviour
{
    DragAndDrop dad;
    Chess chess;
    public Rules()
    {
        dad = new DragAndDrop();
        chess = new Chess();
        
    }

    // Use this for initialization
	void Start ()
    {
        ShowFigures();
        MarkValidFigures();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(dad.Action())
        {
            string from = GetSquare(dad.pickPosition);
            string to = GetSquare(dad.dropPosition);
            Debug.Log(((int)dad.pickPosition.x).ToString() + ";" + ((int)dad.pickPosition.y).ToString());
            string figure = chess.GetFigureAt(((int)dad.pickPosition.x) / 2, ((int)dad.pickPosition.y) / 2).ToString();
            string move = figure + from + to;
            Debug.Log(move);
            chess = chess.Move(move);
            ShowFigures();
            MarkValidFigures();
        }
	}

    public void Reset()
    {
        ShowFigures();
    }

    string GetSquare(Vector2 position) // e2
    {
        int x = Convert.ToInt32(position.x / 2.0);
        int y = Convert.ToInt32(position.y / 2.0);

        return ((char)('a' + x)).ToString() + (y + 1).ToString();
    }

    void ShowFigures()
    {
        int nr = 0;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                string figure = chess.GetFigureAt(x, y).ToString();
                if (figure == ".") continue;
                PlaceFigure("box" + nr, figure, x, y);
                nr++;
            }
        for (; nr < 32; nr++)
            PlaceFigure("box" + nr, "q", 9, 9);
    }

    void MarkValidFigures()
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                MarkSquare(x, y, false);
        foreach (string moves in chess.GetAllMoves())
        {
            // Pe2e4
            int x, y;
            GetCoord(moves.Substring(1, 2), out x, out y);
            MarkSquare(x, y, true);
        }
        
    }

    public void GetCoord(string name, out int x, out int y)
    {
        x = 9;
        y = 9;
        if (name.Length == 2 &&
            name[0] >= 'a' && name[0] <= 'h' &&
            name[1] >= '1' && name[1] <= '8')
        {
            x = name[0] - 'a';
            y = name[1] - '1';
        }
    }

    void PlaceFigure(string box, string figure, int x, int y)
    {
        //Debug.Log(box + " " + figure + " " + x + y);
        GameObject goBox = GameObject.Find(box);
        GameObject goFigure = GameObject.Find(figure);
        GameObject goSquare = GameObject.Find("" + y + x);

        var spriteFigure = goFigure.GetComponent<SpriteRenderer>();
        var spriteBox = goBox.GetComponent<SpriteRenderer>();
        spriteBox.sprite = spriteFigure.sprite;

        goBox.transform.position = goSquare.transform.position;
    }

    void MarkSquare(int x, int y, bool isMarked)
    {
        GameObject goSquare = GameObject.Find("" + y + x);
        GameObject goCell;
        string color = (x + y) % 2 == 0 ? "Black" : "White";
        if (isMarked)
            goCell = GameObject.Find(color + "SquareMarked");
        else
            goCell = GameObject.Find(color + "Square");

        var spriteSquare = goSquare.GetComponent<SpriteRenderer>();
        var spriteCell = goCell.GetComponent<SpriteRenderer>();
        spriteSquare.sprite = spriteCell.sprite;
    }
}

class DragAndDrop
{
    enum State
    {
        none,
        drag
    }

    public Vector2 pickPosition { get; private set; }
    public Vector2 dropPosition { get; private set; }

    State state;
    GameObject item;
    Vector2 offset;

    public DragAndDrop()
    {
        state = State.none;
        item = null;
    }

    public bool Action()
    {
        switch (state)
        {
            case State.none:
                if (IsMouseButtonPressed())
                    PickUp();
                break;
            case State.drag:
                if (IsMouseButtonPressed())
                    Drag();
                else {
                    Drop();
                    return true;
                }
                break;
        }
        return false;
    }

    bool IsMouseButtonPressed()
    {
        return Input.GetMouseButton(0);
    }

    void PickUp()
    {
        Vector2 clickPosition = GetClickposition();
        Transform clickedItem = GetItemAt(clickPosition);
        if (clickedItem == null) return;
        pickPosition = clickedItem.position;
        item = clickedItem.gameObject;
        state = State.drag;
        offset = pickPosition - clickPosition;
        //Debug.Log("picked up: " + item.name);
    }

    Vector2 GetClickposition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    Transform GetItemAt(Vector2 position)
    {
        RaycastHit2D[] figures = Physics2D.RaycastAll(position, position, 0.5f);
        if (figures.Length == 0)
            return null;
        return figures[0].transform;
    }

    void Drag()
    {
        item.transform.position = GetClickposition() + offset;
    }

    void Drop()
    {
        dropPosition = item.transform.position;
        state = State.none;
        item = null;
    }
}
