/* ListBox 1.1                          */
/* April 1, 2015                        */
/* By Orbcreation BV                    */
/* Richard Knol                         */
/* info@orbcreation.com                 */
/* games, components and freelance work */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public delegate void ValueChangedMethod(ListBox aListBox);

public class ListBox : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {   // for now no IpointerClickHandler

	public int[] 		optionInts;
	public string[] 	optionStrings;
	public Sprite[] 	optionSprites;
	public GameObject 	closedListBox;
	public GameObject 	openedListBox;
	public GameObject 	scrollableArea;
	public Scrollbar 	scrollbar;
	public GameObject 	optionTemplate;
	public GameObject 	highlightedOption;
	public GameObject 	hoverOption;

	public bool			alwaysOpen = false;
	public bool			changeTitleToSelectedOption = true;
	public float  		scrollWheelSpeed = 10f;

	public	int 		valueInt;
	public	string 		valueString;
	public	Sprite 		valueSprite;

	public ValueChangedMethod onValueChanged;

	private string 			defaultTitle = "";
	private Sprite 			defaultSprite = null;

	private bool 			isOpen = false;
	private bool 			isDragging = false;
	private Vector2 		openedSize = new Vector2(100,20);
	private Vector2 		optionSize = new Vector2(100,20);

	private GameObject[] 	options;
	private int 			nrOfOptions = 0;
	private int 			nrOfVisibleOptions = 0;
	private float 			optionDistance = 0f;
	private float 			mouseScroll = 0f;
	private float 			yScroll = 0f;
	private float 			oldYScroll = 0f;
	private float 			yScrollSpeed = 0f;
	private float 			scrollAreaHeight = 0f;
	private int 			scrollIndexOffset = 0;
	private float 			lastPointerEventTime = 0f;
	private float 			pointerDownTime = 0f;
	private Vector2 		pointerDownPosition = new Vector2(0,0);
	private float 			minClickTime = 0.5f; // 0.5 seconds between pointerDown and pointerUp for click
	private float 			minClickMovement = 5f; // 5 pixels tolerance to handle pointerUp as click
	private int             hoverIndex = -1;
	private Camera 			lastUsedCamera = null;

	void Start()
	{
        Transform subObject = closedListBox.transform.Find("Text");
        if(subObject != null) {
        	Text txt = subObject.gameObject.GetComponent<Text>();
        	if(txt != null) defaultTitle = txt.text;
	    }
		subObject = closedListBox.transform.Find("Image");
        if(subObject != null) {
        	Image img = subObject.gameObject.GetComponent<Image>();
        	if(img != null) defaultSprite = img.sprite;
		}
		if(scrollbar != null) scrollbar.onValueChanged.AddListener(OnScrollbarChanged);
		Reset();
	}
	public void Reset() 
	{
		SetOptions(optionInts, optionStrings, optionSprites);

		RectTransform rectTrans = scrollableArea.GetComponent<RectTransform>();
		openedSize = rectTrans.rect.size;

		RectTransform optionRectTrans = optionTemplate.GetComponent<RectTransform>();
		optionSize = optionRectTrans.rect.size;

		RectTransform hlOptionRectTrans = highlightedOption.GetComponent<RectTransform>();
		optionDistance = Mathf.Abs(optionRectTrans.offsetMax.y - hlOptionRectTrans.offsetMax.y);

		yScroll = 0f;
		highlightedOption.SetActive(false);
		optionTemplate.SetActive(false);
		SetSelectedIndex(-1);
	}

	void Update()
	{
		if(hoverIndex >=0) mouseScroll += Input.mouseScrollDelta.y * scrollWheelSpeed;
		yScroll += mouseScroll * Time.deltaTime;  // apply mouse scrollspeed

		yScrollSpeed = Mathf.Clamp(yScrollSpeed, -750f, 750f);  // clamp to prevent insane big scrollspeeds
		yScroll += yScrollSpeed * Time.deltaTime;  // apply drag scrollspeed

		if(yScroll<0) {  // bounce back when scrolled to the top
			yScroll += Mathf.Sqrt(yScroll * -50f * Time.deltaTime);
			if(yScrollSpeed<0f && (!isDragging)) yScrollSpeed *= 1f - (4f * Time.deltaTime);
			else yScrollSpeed *= 1f - (1f * Time.deltaTime);
		}
		if(yScroll > (scrollAreaHeight - openedSize.y)) {  // bounce back when scrolled to bottom
			yScroll -= Mathf.Sqrt((yScroll - (scrollAreaHeight - openedSize.y)) * 50f * Time.deltaTime);
			if(yScrollSpeed>0f && (!isDragging)) yScrollSpeed *= 1f - (4f * Time.deltaTime);
			else yScrollSpeed *= 1f - (1f * Time.deltaTime);
		}

		// Fade scroll speed out
		yScrollSpeed *= 1f - (0.5f * Time.deltaTime);
		if(Mathf.Abs(yScrollSpeed) < 0.5f) yScrollSpeed = 0f;  // prevent miniscule scroll amounts
		// Fade scroll speed out
		mouseScroll = Mathf.Lerp(mouseScroll, 0f, 2f * Time.deltaTime);
		if(Mathf.Abs(mouseScroll) < 0.5f) mouseScroll = 0f;  // prevent miniscule scroll amounts

		// Only compute the options to show when changed significantly
		if(Mathf.Abs(oldYScroll - yScroll) > 0.5f) { 
			oldYScroll = yScroll;
		}

		if(alwaysOpen && !isOpen) OpenListBox();
		if(isOpen) {
			if(scrollbar != null) {
				if((scrollAreaHeight - openedSize.y) > 0f) scrollbar.value = yScroll / (scrollAreaHeight - openedSize.y);
				if(scrollAreaHeight > 0f) scrollbar.size = Mathf.Clamp01(openedSize.y / scrollAreaHeight);
			}
			// Close the listbox when another object is selected
			GameObject selObj = EventSystem.current.currentSelectedGameObject;
			if(selObj != gameObject && selObj != scrollableArea && (scrollbar == null || selObj != scrollbar.gameObject)) {
				CloseListBox();
			}

			OnPointerMoved(Input.mousePosition);
			ShowOptions();
		}
	}

	// Bunch of overloaded methods to set the listbox options
	public void SetOptions(Sprite[] sprites) {
		SetOptions(null, null, sprites);
	}
	public void SetOptions(string[] strings) {
		SetOptions(null, strings, null);
	}
	public void SetOptions(int[] ints) {
		// When you only set an array of ints, we have to create an array of strings to display the values
		if(ints == null || ints.Length <= 0) return;
		string[] strings = new string[ints.Length];
		for(int i=0;i<ints.Length;i++) strings[i] = "" + ints[i];
		SetOptions(ints, strings, null);
	}
	public void SetOptions(int[] ints, Sprite[] sprites) {
		SetOptions(ints, null, sprites);
	}
	public void SetOptions(int[] ints, string[] strings) {
		SetOptions(ints, strings, null);
	}
	public void SetOptions(string[] strings, Sprite[] sprites) {
		SetOptions(null, strings, sprites);
	}

	public void SetOptions(int[] ints, string[] strings, Sprite[] sprites) {
		// we have to close the listbox when you set the option values 
		CloseListBox();

		// obtain the number of options
		nrOfOptions = 0;
		if(ints != null && ints.Length>0) nrOfOptions = ints.Length;
		else if(strings != null && strings.Length>0) nrOfOptions = strings.Length;
		else if(sprites != null && sprites.Length>0) nrOfOptions = sprites.Length;

		// Make sure the list is filled with a minimal number of options. if not, the first item will appear somewhere in the middel instead of at the top of the opened listbox
		nrOfVisibleOptions = 5;
		if(optionDistance > 0f) nrOfVisibleOptions = Mathf.CeilToInt(openedSize.y / optionDistance) + 1;
		if(nrOfOptions < nrOfVisibleOptions-1) {
			if(strings != null && strings.Length>0) {
				int i = 0;
				string[] newStrings = new string[nrOfVisibleOptions-1];
				for(i=0;i<strings.Length;i++) newStrings[i] = strings[i];
				for(;i<nrOfVisibleOptions-1;i++) newStrings[i] = "";
				strings = newStrings;
				nrOfOptions = nrOfVisibleOptions-1;
			}
			if(sprites != null && sprites.Length>0) {
				int i = 0;
				Sprite[] newSprites = new Sprite[nrOfVisibleOptions-1];
				for(i=0;i<sprites.Length;i++) newSprites[i] = sprites[i];
				for(;i<nrOfVisibleOptions-1;i++) newSprites[i] = null;
				sprites = newSprites;
				nrOfOptions = nrOfVisibleOptions-1;
			}
		}

		// length of all arrays should be the same
		if(strings != null && strings.Length>0 && strings.Length != nrOfOptions) {
			Debug.Log("Length of string array(" + strings.Length + ") not equal to nr of options("+nrOfOptions+")");
			return;
		}
		if(sprites != null && sprites.Length>0 && sprites.Length != nrOfOptions) {
			Debug.Log("Length of sprite array(" + sprites.Length + ") not equal to nr of options("+nrOfOptions+")");
			return;
		}

		// store the new arrays as option values
		optionInts = ints;
		optionStrings = strings;
		optionSprites = sprites;
	}

	// Find the index of the currently selected option 
	// returns -1 if no option is selected
	public int GetSelectedIndex() {
		if(valueInt > 0) {
			for(int i=0;i<optionInts.Length;i++) {
				if(optionInts[i] == valueInt) return i;
			}
		}
		if(valueString != null && valueString.Length > 0) {
			for(int i=0;i<optionStrings.Length;i++) {
				if(optionStrings[i] == valueString) return i;
			}
		}
		if(valueSprite != null) {
			for(int i=0;optionSprites != null && i<optionSprites.Length;i++) {
				if(optionSprites[i] == valueSprite) return i;
			}
		}
		return -1;
	}

	public void SetValue(int aValue) 
	{
		valueInt = aValue;
		int idx = 0;
		for(int i=0;i<optionInts.Length;i++) {
			if(optionInts[i] == valueInt) idx = i;
		}
		SetSelectedIndex(idx);
	}
	public void SetValue(string aValue) 
	{
		valueString = aValue;
		int idx = 0;
		for(int i=0;i<optionStrings.Length;i++) {
			if(optionStrings[i] == valueString) idx = i;
		}
		SetSelectedIndex(idx);
	}
	public void SetValue(Sprite aSprite) 
	{
		valueSprite = aSprite;
		int idx = 0;
		for(int i=0;optionSprites != null && i<optionSprites.Length;i++) {
			if(optionSprites[i] == valueSprite) idx = i;
		}
		SetSelectedIndex(idx);
	}
	// Set valueInt, valueString and valueSprite to match the index of a newly selected option
	// Will also trigger the onValueChanged delegate
	public void SetSelectedIndex(int idx) 
	{
		int oldIdx = GetSelectedIndex();
		valueInt = 0;
		valueString = null;
		valueSprite = null;
		if(optionInts != null && idx>=0 && idx < optionInts.Length) {
			valueInt = optionInts[idx];
		}
		if(optionStrings != null && idx>=0 && idx < optionStrings.Length) {
			valueString = optionStrings[idx];
		}
		if(optionSprites != null && idx>=0 && idx < optionSprites.Length) {
			valueSprite = optionSprites[idx];
		}

		string titleToShow = defaultTitle;
		Sprite spriteToShow = defaultSprite;
		if(changeTitleToSelectedOption) {
			if(valueString != null) titleToShow = valueString;
			if(valueSprite != null) spriteToShow = valueSprite;
		}
        Transform subObject = closedListBox.transform.Find("Text");
        if(subObject != null) {
        	Text txt = subObject.gameObject.GetComponent<Text>();
        	if(txt != null) {
        		if(titleToShow != null) {
	        		txt.text = titleToShow;
	        		txt.enabled = true;
	        	} else txt.enabled = false;
		    }
	    }
		subObject = closedListBox.transform.Find("Image");
        if(subObject != null) {
        	Image img = subObject.gameObject.GetComponent<Image>();
        	if(img != null) {
        		if(spriteToShow != null) {
	        		img.sprite = spriteToShow;
	        		img.enabled = true;
	        	} else img.enabled = false;
	        }
		}
		if(idx!=oldIdx && onValueChanged != null) {
			onValueChanged(this);
		}
	}

	public void OpenListBox() {
		SetOpen(true);
	}
	public void CloseListBox() {
		if(!alwaysOpen) SetOpen(false);
	}

	// This title will be shown if no option is selected or when changeTitleToSelectedOption == false
	public void SetTitle(string aTitle) {
		defaultTitle = aTitle;
	}
	// This sprite will be shown if no option is selected or when changeTitleToSelectedOption == false
	public void SetSprite(Sprite aSprite) {
		defaultSprite = aSprite;
	}

	// Catching events
	public void OnPointerDown(PointerEventData data) {
		lastPointerEventTime = Time.time;
		// We handle clicking manually, because OnPointerClick is too trigger happy 
		// at the time of writing this code
		pointerDownTime = Time.time;
		pointerDownPosition = data.position;
		lastUsedCamera = data.enterEventCamera;
	}
	public void OnPointerUp(PointerEventData data) {
		float deltaTime = Time.time - pointerDownTime;
		if(deltaTime < minClickTime && Vector2.Distance(pointerDownPosition, data.position) < minClickMovement) {
			// send it manually because the new EventSystem doesnt seem to work properly
			// at the time of writing this code
			// OnPointerClick is too trigger happy for my taste
			OnPointerClick(data);  
		}
	}
	// We invoke this manually, because IOnPointerClick is too trigger happy 
	// at the time of writing this code
	public void OnPointerClick(PointerEventData data) {
		if(isOpen) {
			for(int i=0;i<nrOfVisibleOptions;i++) {
				RectTransform optionTransform = options[i].GetComponent<RectTransform>();
				if(RectTransformUtility.RectangleContainsScreenPoint(optionTransform, data.position, data.enterEventCamera)) {
					SetSelectedIndex(i + scrollIndexOffset);
				}
			}
			ShowOptions();
			Invoke("CloseListBox", 0.3f);
		} else OpenListBox();
	}

	// This is invoked by catching Input.mousePosition in the Update function
	public void OnPointerMoved(Vector3 newPosition) {
		hoverIndex = -1;
		if(isOpen) {
			for(int i=0;i<nrOfVisibleOptions;i++) {
				RectTransform optionTransform = options[i].GetComponent<RectTransform>();
				if(RectTransformUtility.RectangleContainsScreenPoint(optionTransform, newPosition, lastUsedCamera)) {
					hoverIndex = i + scrollIndexOffset;
				}
			}
		}
	}


	public void OnBeginDrag(PointerEventData data) {
		isDragging = true;
	}

	public void OnEndDrag(PointerEventData data) {
		isDragging = false;
	}

	public void OnDrag(PointerEventData data) {
		float deltaTime = Time.time - lastPointerEventTime;
		if(deltaTime>0f) yScrollSpeed = (yScrollSpeed + (data.delta.y / deltaTime)) * 0.5f;
		lastPointerEventTime = Time.time;
	}

	public void OnScrollbarChanged(float newValue) {
		yScrollSpeed = 0f;
		yScroll = (scrollAreaHeight - openedSize.y) * newValue;
	}

	// Set up/clear an array of visible options, and switch view between closedListBox and openedListBox
	private void SetOpen(bool yn) {
		if(isOpen == yn) return;
		closedListBox.SetActive(!yn);
		openedListBox.SetActive(yn);
		if(yn) {
	        scrollAreaHeight = nrOfOptions * optionDistance;
			nrOfVisibleOptions = 5;
			if(optionDistance > 0f) nrOfVisibleOptions = Mathf.CeilToInt(openedSize.y / optionDistance) + 1;

			float y = 0f;
			options = new GameObject[nrOfVisibleOptions];
			for(int i=0;i<nrOfVisibleOptions;i++) {
				options[i] = (GameObject)GameObject.Instantiate(optionTemplate);
				RectTransform optionTransform = options[i].GetComponent<RectTransform>();
				optionTransform.SetParent(scrollableArea.transform);
				optionTransform.localScale = new Vector3(1f ,1f, 1f);
				optionTransform.pivot = new Vector2(0f ,1f);
		        optionTransform.anchorMin = new Vector2(0f, 1f);
		        optionTransform.anchorMax = new Vector2(0f, 1f);
		    	FixLocalPositionAndRotation(optionTransform);
		        y -= optionDistance;
				options[i].SetActive(true);
		    }

			RectTransform highlightedOptionTransform = highlightedOption.GetComponent<RectTransform>();
		    highlightedOptionTransform.SetParent(scrollableArea.transform);
			highlightedOptionTransform.localScale = new Vector3(1f ,1f, 1f);
			highlightedOptionTransform.pivot = new Vector2(0f ,1f);
		    highlightedOptionTransform.anchorMin = new Vector2(0f, 1f);
		    highlightedOptionTransform.anchorMax = new Vector2(0f, 1f);
	      	FixLocalPositionAndRotation(highlightedOptionTransform);

	      	if(hoverOption != null) {
				RectTransform hoverOptionTransform = hoverOption.GetComponent<RectTransform>();
			    hoverOptionTransform.SetParent(scrollableArea.transform);
				hoverOptionTransform.localScale = new Vector3(1f ,1f, 1f);
				hoverOptionTransform.pivot = new Vector2(0f ,1f);
			    hoverOptionTransform.anchorMin = new Vector2(0f, 1f);
			    hoverOptionTransform.anchorMax = new Vector2(0f, 1f);
		      	FixLocalPositionAndRotation(hoverOptionTransform);
		    }

			isOpen = yn;
			ShowOptions();

		} else {
			isOpen = yn;
			ClearAllOptions();
		}
	}

	// Compute positions and contents of the visible options.
	// The listbox uses only a handfull of UI option elements to cover the visible area.
	// The options value array can be much longer.
	// This functions makes sure only the currently visible option values are placed inside the UI elements
	// and displayed in the right place. This will make it seem like a very long list, which it is not.
	private void ShowOptions() {
		if(!isOpen) return;
		int selectedIndex = GetSelectedIndex();  // get the index of the selected option
		scrollIndexOffset = 0;
		float y = yScroll;  // set y to match the current scroll position

		hoverOption.SetActive(false);   // deactivate the hover option (will be activated later if needed)
		highlightedOption.SetActive(false);   // deactivate the highlighted option (will be activated later if needed)

		// compute the index of the first visible option and the matching y coordinate;
		scrollIndexOffset = Mathf.FloorToInt(Mathf.Max(y / optionDistance, 0f));
		y -= scrollIndexOffset * optionDistance;

		// loop through the visible options
		for(int i=0;i<nrOfVisibleOptions;i++) {
			GameObject option = options[i];

			// if this option matches the currently selected index or the hoverIndex,
			// we disable the normal option and use the highlightedOption instead
			if(i + scrollIndexOffset == hoverIndex && hoverOption != null) {  
				option.SetActive(false);
				option = hoverOption;
			}
			if(i + scrollIndexOffset == selectedIndex) {  
				option.SetActive(false);
				option = highlightedOption;
			}

			option.SetActive(true);  // make active in case it was previously disabled

			// Set the position and size of the option
			RectTransform optionTransform = option.GetComponent<RectTransform>();
	        optionTransform.offsetMin = new Vector2(0f, y - optionSize.y);
	        optionTransform.offsetMax = new Vector2(openedSize.x, y);
		    FixLocalPositionAndRotation(optionTransform);

	        // Set the contents
	        Transform subObject = option.transform.Find("Text");
	        if(subObject != null) {
	        	Text txt = subObject.gameObject.GetComponent<Text>();
	        	if(txt != null) {
		        	if(optionStrings.Length > i+scrollIndexOffset) {
			        	txt.enabled = true;
			        	txt.text = optionStrings[i+scrollIndexOffset];
			        } else {
			        	txt.enabled = false;
			        }
			    }
		    }
			subObject = option.transform.Find("Image");
	        if(subObject != null) {
	        	Image img = subObject.gameObject.GetComponent<Image>();
	        	if(img != null) {
		        	if(optionSprites != null && optionSprites.Length > i+scrollIndexOffset) {
			        	img.enabled = true;
		        		img.sprite = optionSprites[i+scrollIndexOffset];
		        	} else {
		        		img.enabled = false;
		        	}
		        }
			}

			// move y to next option
	        y -= optionDistance;
		}
	}

	private void ClearAllOptions() {
		if(options == null) return;  // nothing to clear

		// destroy the option gameobjects
		for(int i=options.Length-1;i>=0;i--) {
			if(options[i] != null) GameObject.Destroy(options[i]);
		}
		options = null;
	}

	private void FixLocalPositionAndRotation(RectTransform aTransform) {
	    Vector3    pos = aTransform.localPosition;
	    pos.z = 0f;
	    aTransform.localPosition = pos;
	    aTransform.localRotation = Quaternion.identity;
	}
}

