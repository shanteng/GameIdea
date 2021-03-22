System( "AutoCodeTestSystem", SystemScope.Hall) 
 
PanelDefine = 
{
	File ="AutoCodeTest_Panel",
	
	Windows = 
	{
		{
			Name = "XUiButton",
			Type = "Button",
			Alias = "XUiButton",
			Handles = 
			{
				["OnClick"] = "OnXUiButtonClicked",
			},
		},
	
		{
			Name = "XUiButton/Text",
			Type = "Window",
			Alias = "Text",
		},
	
		{
			Name = "XUiLabel",
			Type = "Label",
			Alias = "XUiLabel",
		},
	
		{
			Name = "XUiSlider",
			Type = "Slider",
			Alias = "XUiSlider",
		},
	
		{
			Name = "EasyListView",
			Type = "ListView",
			Alias = "EasyListView",
			Handles = 
			{
				["ListViewUpdated"] = "OnEasyListViewListViewUpdated",
				["ListItemUpdated"] = "OnEasyListViewListItemUpdated",
			},
			CacheSetting = 
			{
				Type = "DefaultItem",
				Handles = 
				{
					--Your Cache Handle Code
				},
			}
		},
	
		{
			Name = "XUiDynImage",
			Type = "DynamicImage",
			Alias = "XUiDynImage",
		},
	
	
	}
}

function OnInit(self)

end

function OnShow(self)

end

function OnHide(self)

end

function OnRelease(self)

end

function OnXUiButtonClicked(self)

end

function OnEasyListViewListViewUpdated(self)

end

function OnEasyListViewListItemUpdated(self)

end

