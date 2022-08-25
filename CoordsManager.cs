using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class CoordsManager : NetworkBehaviour
{
    [SyncVar] public int x_coord, y_coord;
    public TextMeshProUGUI x_text, y_text;
    public int x_step = 1, y_step = 1;
    public GameObject neededCoordsObject;
    public TextMeshProUGUI neededCoords;
    [SyncVar(hook=nameof(OnChangeNeeded))] public int needed_x, needed_y;

    // Update is called once per frame
    void Update()
    {
        x_text.text = x_coord.ToString();
        y_text.text = y_coord.ToString();
    }

    public void X_up()
    {
        ChangeCoords("x", x_step);
    }

    public void X_down()
    {
        ChangeCoords("x", -x_step);
    }

    public void Y_up()
    {
        ChangeCoords("y", y_step);
    }

    public void Y_down()
    {
        ChangeCoords("y", -y_step);
    }

    public void ChangeCoords(string coord, int step)
    {
        CmdChangeCoords(coord, step);
    }

    [Command(requiresAuthority=false)]
    void CmdChangeCoords(string coord, int step)
    {
        if (coord == "x") x_coord += step; else if (coord == "y") y_coord += step;
    }

    public void SetNeededCoords(int new_x, int new_y)
    {
        if (isServer) CmdSetCoords(new_x, new_y);
    }

    [Command(requiresAuthority = false)]
    void CmdSetCoords(int new_x, int new_y)
    {
        neededCoords = neededCoordsObject.GetComponent<TextMeshProUGUI>();
        needed_x = new_x; needed_y = new_y;
    }

    void OnChangeNeeded(int oldY, int newY)
    {
        neededCoords.text = needed_x.ToString() + " ; " + needed_y.ToString();
    }

    public bool CheckCoords()
    {
        Debug.Log("needed: " + needed_x + " " + needed_y + " / got: " + x_coord + " " + y_coord);
        if ((needed_x == x_coord) && (needed_y == y_coord))
        {
            Debug.Log("true");
            return true;
        }
        else
        {
            Debug.Log("false");
            return false;
        }
    }
}
