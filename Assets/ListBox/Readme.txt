Listbox:
--------
Add a dropdown listbox to your UI canvas.

A menu item is added to easily instantiate the list box. You can populate it with an array of strings and/or sprites.

The appearance listbox can be configured with the unity UI system. It has separate UI objects for the closed state of the listbox, the opened state, the options and the highlighted option.

You can use large option arrays if you want, because only the visible options are represented by gameobjects who's display values are swapped as you scroll through the listbox.

All C# source code and demo scene included. The documentation can be viewed on our website http://orbcreation.com/orbcreation/docu.orb?1072

Note: This package only works with the new Unity UI system, not with OnGUI().


Configuring the listbox:
------------------------
When you add a Listbox to your scene you can change the appearance of the listbox by editing 
the underlying gameobjects.

The listbox consists of 5 parts:
- closed listbox
- opened listbox
- option
- highlighted option
- hover option

Each of these parts can be configured as you see fit, but there are a few rules:
- The top level gameObject needs to contain the ListBox script
- If the closed listbox has a child called "Text", this will be used to set the title. 
  So do not rename this.
- If the closed listbox has a child called "Image", this will be used to set the selected sprite.

- The opened listbox needs to contain the ListBoxEventCatcher script.
- The Scrollable area inside the Opened listbox defines the size of the visible part of the option
  list when the listbox is open.
- The Scrollable area inside the Opened listbox should have a Mask and a Image Component to obscure 
  the options beyond the area's borders.
 
- Option and Highlighted option (inside Opened listbox) should have a child called "Text" if you 
  plan to show strings
- Option and Highlighted option  (inside Opened listbox) should have a child called "Image" if you 
  plan to show sprites

The distance between Highlighted option and Option is measured upon startup and will define the 
spacing between the options in the listbox.


Script interface:
-----------------
Properties:
-----------
public int[] optionInts
	Array of ints to be used as identifiers for the listbox options. These will not be displayed.

public string[] 	optionStrings;
	Array of strings to be shown in Option/Text

public Sprite[] 	optionSprites;
	Array of spites to be shown in Option/Image

public GameObject 	closedListBox;
	Reference to the closed listbox. Will Be set active/inactive when the listbox opens/closes

public GameObject 	openedListBox;
	Reference to the opened listbox. Will Be set active/inactive when the listbox opens/closes

public GameObject 	scrollableArea;
	Reference to the opened listbox. Will be used to measure the dimensions of the content area

public GameObject 	optionTemplate;
	Reference to the template for options. Will be instantiated the number of times needed to fill
	scrollableArea + 1

public GameObject 	highlightedOption;
	Reference to highlighted option. Will be made active/inactive to show the selected value

public bool			changeTitleToSelectedOption = true;
	If set to true, the children of closedListBox named "Text" and "Image" will be set to match the 
	selected option. If no option is selected, they will be set to the default string and default 
	sprite (if set) 

public	int 		valueInt
	Read the currently selected value.

public	string 		valueString;
	Read the currently selected value

public	Sprite 		valueSprite;
	Read the currently selected value

public ValueChangedMethod onValueChanged;
	Set a delegate method to call when the selected option changes. 
	Method should look like: void ValueChangedMethod(ListBox aListBox). See Demo script for an 
	example.

Methods:
--------
public void SetOptions(int[] ints, string[] strings, Sprite[] sprites)
	Set the option values for the listbox. The parameters are optional, but you need to specify at 
	least one of them. 
	Example:
	   myListBox.SetOptions(new string[4] {"Ketchup", "Mayo", "BBQ", "Curry"} );

public int GetSelectedIndex()
	Find the index of the currently selected option. Returns -1 if no option is selected

public void SetSelectedIndex(int idx) 
	Set valueInt, valueString and valueSprite to match the index of a newly selected option. Will 
	also trigger the onValueChanged delegate

public void OpenListBox()
	Open the listbox programmatically

public void CloseListBox()
	Close the listbox programmatically

public void SetTitle(string aTitle)
	Set the default tiltle that will be shown on closedListbBox/Text when no option is selected or 
	when changeTitleToSelectedOption == false

public void SetSprite(Sprite aSprite)
	Set the default sprite that will be shown on closedListBox/Image when no option is selected or 
	when changeTitleToSelectedOption == false

public void Reset()
    Programmatically clear the option arrays and clear any selected values.

