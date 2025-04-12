using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Card Object Creator and Manager
public class Card
{
    // varibales relevant to cards
    public string cardName;
    public string description;
    public int hp;
    
    // assign variables to card object
    public Card(string n, string d, int h) {
        cardName = n;
        description = d;
        hp = h;
    }

    // getters for card variables
    public string getName() { return cardName;}
    public string getDescription() { return description;}
    public int getHP() { return hp;}
}
