﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polices
{
	public class PoliceActionManager : MonoBehaviour
	{
		//自分の情報
		PoliceParams policeParams;
		private List<List<PoliceAction>> m_ActionLists;
		public string m_nowAction;
		public string m_beforeAction;
		private TextAsset basicCommands;
		private TextAsset otherCommand;
		private TextAsset preferencialCommands;
		[SerializeField]
		List<string> basicBehaviors;

		//その他オブジェクトの情報
		EventManager eventManager;


		void Start ()
		{
			policeParams = GetComponent<PoliceParams> ();
			eventManager = GameObject.FindObjectOfType<EventManager>();
			LoadCommands ();
			m_ActionLists = new List<List<PoliceAction>> ();
			GetBasicBehaviors ();

			policeParams.policeStatus = PoliceStatus.BASIC_BEHAVIOR;
			m_beforeAction = "Work";
			SetNextAction ("InstantiatedToMyDesk", 0);
		}

		void LoadCommands ()
		{
			basicCommands = Resources.Load ("Commands/BasicCommands") as TextAsset;
			otherCommand = Resources.Load ("Commands/OtherCommands") as TextAsset;
			preferencialCommands = Resources.Load ("Commands/PreferancialCommands") as TextAsset;
		}

		public string GetNowAction ()
		{
			return m_nowAction;
		}

		private void GetBasicBehaviors ()
		{
			string cmd = basicCommands.ToString ();

			var lines = cmd.Split ('\n');
			foreach (var cmdLine in lines) {
				string[] cmds = cmdLine.Split (',');
				if (cmds [0] == ":") {
					basicBehaviors.Add (cmds [1]);
					continue;
				}
			}
		}

		public void RecieveEvents(string name){
			if(name == "Poop"){
				SetNextAction ("CleanPoop", 1);
			}
		}

		private void ThinkNextAction ()
		{
			string actionName;
			if (m_nowAction == "Work") {
				actionName = basicBehaviors [Random.Range (0, basicBehaviors.Count)];
				Debug.Log(actionName);
				foreach (var item in eventManager.policeActionManagers){
					if(item.m_nowAction  == actionName){
						actionName = "Work";
						break;
					}
				}
			} else {
				actionName = "Work";
			}
			policeParams.policeStatus = PoliceStatus.BASIC_BEHAVIOR;
			SetNextAction (actionName, 0);
		}

		/// <summary>
		/// Sets the next action.(Action type -- 1:BasicAction, 2:PreferencialAction)
		/// </summary>
		private void SetNextAction (string actionName, int actionType)
		{
			string cmd = basicCommands.ToString () + otherCommand.ToString () + preferencialCommands.ToString ();

			var actionList = new List<PoliceAction> ();
			var lines = cmd.Split ('\n');
			bool actionFound = false;
			foreach (var cmdLine in lines) {
				string[] cmds = cmdLine.Split (',');

				if (!actionFound) {
					if (string.IsNullOrEmpty (cmdLine) || cmds [0] != ":") {
						continue;
					} else {
						if (cmds [1] == actionName) {
							Debug.Log (actionName);
							m_beforeAction = m_nowAction;
							m_nowAction = actionName;
							actionFound = true;
							continue;
						}
					}
				} else {
					if (cmds [0] == "::") {
						break;
					}
					switch (cmds [0]) {
					case "PlayAnimation":
						PlayAnimation (actionList, cmds [1], System.Boolean.Parse (cmds [2]));
						break;
					case "WalkTo":
						WalkTo (actionList, cmds [1], int.Parse (cmds [2]), policeParams.policeID);
						break;
					case "RotateTo":
						RotateTo (actionList, cmds [1], float.Parse (cmds [2]), policeParams.policeID);
						break;
					case "WaitAction":
						WaitAction (actionList);
						break;
					case "WaitForSeconds":
						WaitForSeconds (actionList, float.Parse (cmds [1]));
						break;
					}
				}
			}
			if (actionType == 0) {
				NextAction (actionList);
			} else if (actionType == 1) {
				InterruptAction (actionList);
			}
		}

		private void NextAction (List<PoliceAction> actionList)
		{
			if (m_ActionLists.Count == 0) {
				m_ActionLists.Add (actionList);
			} else {
				m_ActionLists [0] = actionList;
			}
		}

		private void InterruptAction (List<PoliceAction> actionList)
		{
			if (m_ActionLists.Count != 0) {
				var activeAction = m_ActionLists [m_ActionLists.Count - 1];
				foreach (var act in activeAction) {
					if (act is WaitAction) {
						break;
					}
					act.Suspend ();
				}
			}
			policeParams.policeStatus = PoliceStatus.PREFERENCIAL_BEHAVIOR;
			m_ActionLists.Add (actionList);
		}

		public void Update ()
		{
			if (m_ActionLists.Count != 0) {
				var activeAction = m_ActionLists [m_ActionLists.Count - 1];
				var isAllEnd = true;
				foreach (var act in activeAction) {
					if (act is WaitAction) {
						break;
					}
					if (!act.IsStart ()) {
						act.Start ();
						act.SetStart ();
					}
					act.Update ();
					if (!act.IsEnd ()) {
						isAllEnd = false;
					}
				}
				if (isAllEnd) {
					while (activeAction.Count > 0 && !(activeAction [0] is WaitAction)) {
						activeAction.RemoveAt (0);
					}
					activeAction.RemoveAt (0);
					if (activeAction.Count == 0) {
						if (m_ActionLists.Count != 0) {
							m_ActionLists.Remove (activeAction);
							if (m_ActionLists.Count > 0) {
								activeAction = m_ActionLists [m_ActionLists.Count - 1];
							}
							foreach (var act in activeAction) {
								if (act is WaitAction) {
									break;
								}
								act.Resume ();
							}
							if(activeAction.Count > 0){
								Debug.Log("ヨバレタ");
								policeParams.policeStatus = PoliceStatus.BASIC_BEHAVIOR;
								if(m_beforeAction == "Work"){
									//自分のシートに帰るアクションを追加する
									SetNextAction ("BackToMySheet", 0);
								}
								string str = m_nowAction;
								m_nowAction = m_beforeAction;
								m_beforeAction = str;
							}
						}
					}
				}
			}
			if (m_ActionLists.Count == 0) {
				ThinkNextAction ();
			}
		}

		private void WaitAction (List<PoliceAction> list)
		{
			var act = new WaitAction ();
			act.SetGameObject (this.gameObject);
			list.Add (act);
		}

		private void PlayAnimation (List<PoliceAction> list, string anim, bool repeat)
		{
			var act = new StartActionAnimation ();
			act.SetGameObject (this.gameObject);
			act.Init (anim, repeat);
			list.Add (act);
		}

		private void RotateTo (List<PoliceAction> list, Quaternion angle, float speed)
		{
			var act = new RotateTo ();
			act.SetGameObject (this.gameObject);
			act.Init (angle, speed);
			list.Add (act);
		}

		private void RotateTo (List<PoliceAction> list, Vector3 destPos, float speed)
		{
			var act = new RotateTo ();
			act.SetGameObject (this.gameObject);
			act.Init (destPos, speed);
			list.Add (act);
		}

		private void RotateTo (List<PoliceAction> list, string destObj, float speed, int policeId)
		{
			var act = new RotateTo ();
			act.SetGameObject (this.gameObject);
			act.Init (destObj, speed, policeId);
			list.Add (act);
		}

		private void WalkTo (List<PoliceAction> list, Vector3 destPos, float speed)
		{
			var act = new WalkTo ();
			act.SetGameObject (this.gameObject);
			act.Init (destPos, speed);
			list.Add (act);
		}

		private void WalkTo (List<PoliceAction> list, string destObj, float speed, int policeId)
		{
			var act = new WalkTo ();
			act.SetGameObject (this.gameObject);
			act.Init (destObj, speed, policeId);
			list.Add (act);
		}

		private void WaitForSeconds (List<PoliceAction> list, float waitTime)
		{
			var act = new WaitForSeconds ();
			act.SetGameObject (this.gameObject);
			act.Init (waitTime);
			list.Add (act);
		}


	}
}