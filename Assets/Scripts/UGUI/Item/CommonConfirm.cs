using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CommonConfirm : BaseItem
{
	public Text TitleText;
	public Text InfoText;
	public Button ConfirmButton;
	public Button CancelButton;

	public void Show(string title,string content,
		UnityAction confirmAction,UnityAction cancelAction) {
		TitleText.text = title;
		InfoText.text = content;
		AddButtonClickListener(ConfirmButton,()=> {
			confirmAction();
			Destroy(gameObject);
		});
		AddButtonClickListener(CancelButton, () => {
			cancelAction();
			Destroy(gameObject);
		});
	}
}
