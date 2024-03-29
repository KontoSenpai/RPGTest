﻿using RPGTest.Models.Entity;
using RPGTest.Modules.Battle;
using RPGTest.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTest.Modules.Battle.UI
{
    public class UI_BattleOverlay : UI_Base
    {
        [SerializeField] private GameObject ActionWidget;
        [SerializeField] private GameObject[] MultiActionPanels;

        [SerializeField] private GameObject BattleHeader;
        [SerializeField] private GameObject RewardPanel;
        [SerializeField] private TextMeshProUGUI HeaderString;
        [SerializeField] private GameObject[] PartyMemberWidgets;

        [SerializeField] private AudioSource AudioSource;
        [SerializeField] private AudioClip ATBFilledSoundNotification;

        private TargettingSystem m_targettingSystem;

        private List<PlayableCharacter> m_party;

        public event RewardPanelClosedHandler RewardPanelClosed;
        public delegate void RewardPanelClosedHandler();

        public void Initialize(TargettingSystem ts)
        {
            m_targettingSystem = ts;
            ActionWidget.GetComponent<UI_ActionSelection_Widget>().PlayerTargetingRequested += m_targettingSystem.UI_BattleOverlay_PlayerTargetingRequested;
            ActionWidget.GetComponent<UI_ActionSelection_Widget>().MultiCastCountChanged += UI_BattleOverlay_MultiCastCountChanged;
            ActionWidget.GetComponent<UI_ActionSelection_Widget>().MultiCastActionSelected += UI_BattleOverlay_MultiCastActionSelected;
            ActionWidget.GetComponent<UI_ActionSelection_Widget>().ResetMultiCastStateRequested += UI_BattleOverlay_ResetMultiCastStateRequested;
            m_targettingSystem.PlayerTargettingDone += ActionWidget.GetComponent<UI_ActionSelection_Widget>().ValidateTargetInformation;
            
            foreach (var widget in PartyMemberWidgets)
            {
                var widgetScript = widget.GetComponent<UI_PartyMember_Widget>();

                widget.SetActive(false);
            }

            RewardPanel.GetComponent<UI_BattleRewards>().UIClosed += (s, e) => RewardPanelClosed();
        }

        public void ShowActionSelection(PlayableCharacter character)
        {
            AudioSource.PlayOneShot(ATBFilledSoundNotification);
            ActionWidget.GetComponent<UI_ActionSelection_Widget>().Initialize(character);
        }

        private void UI_BattleOverlay_ResetMultiCastStateRequested()
        {
            foreach(var multiActionPanel in MultiActionPanels.Reverse())
            {
                UI_PanelMulticast script = multiActionPanel.GetComponent<UI_PanelMulticast>();
                script.SetEnabled(false);
                script.SetSelected(false);
                script.SetActionName(null);
            }
        }

        private void UI_BattleOverlay_MultiCastCountChanged(int count)
        {
            foreach(var multiActionPanel in MultiActionPanels)
            {
                multiActionPanel.GetComponent<UI_PanelMulticast>().SetEnabled(count >= 2 && MultiActionPanels.ToList().IndexOf(multiActionPanel) < count);
            }
        }

        private void UI_BattleOverlay_MultiCastActionSelected(int index, string actionName)
        {
            var multiActionPanel = MultiActionPanels[index];
            UI_PanelMulticast script = multiActionPanel.GetComponent<UI_PanelMulticast>();
            script.SetSelected(!string.IsNullOrEmpty(actionName));
            script.SetActionName(actionName);
        }

        public void Initialize(List<PlayableCharacter> party)
        {
            m_party = party;
            UIOpened(this, null);

            BattleHeader.SetActive(false);
            RewardPanel.GetComponent<UI_BattleRewards>().Clear();
            RewardPanel.SetActive(false);

            for (int i = 0; i < party.Count; i++)
            {
                var widget = PartyMemberWidgets[i];
                widget.SetActive(true);
                var widgetScript = widget.GetComponent<UI_PartyMember_Widget>();
                widgetScript.Initialize(m_party[i]);
            }

            StartCoroutine(Fade(true, 1));
        }

        public IEnumerator DisplayMessage(string actionString, float duration = -1.0f)
        {
            BattleHeader.SetActive(true);
            HeaderString.text = actionString;
            yield return new WaitForSeconds(duration > - 1.0f ? duration : 3.0f);
            BattleHeader.SetActive(false);
        }

        public void DisplayRewards(int experience, Dictionary<string, int> items, int gold)
        {
            Clean();  
            RewardPanel.SetActive(true);
            RewardPanel.GetComponent<UI_BattleRewards>().DisplayRewards(experience, items, gold);
        }

        public void Clean()
        {
            foreach (var widget in PartyMemberWidgets)
            {
                if(widget.activeInHierarchy)
                    widget.GetComponent<UI_PartyMember_Widget>().DisableEvents();
                widget.SetActive(false);
            }
        }

        public void Close()
        {
            UIClosed(this, null);
        }
    }
}

