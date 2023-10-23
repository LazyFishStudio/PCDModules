using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    public class BackpackUIManager : MonoBehaviour
    {
        private const float radius = 2000f;
        private const float space = 235f;
        private const int maxItemsInScreen = 5;

        public GameObject chooseUI;
        public GameObject chooseDot;

        private bool isActive;
        private Camera UICamera;
        private List<GameObject> holdingItems { get { return BackpackManager.Inst.holdingItems; } }
        private int curSelectIdx { get { return BackpackManager.Inst.selectIdx; } }
        private int curShowOffset;
        private int curDotIdx;

        private void Awake() {
            UICamera = GetComponentInChildren<Camera>();
            curShowOffset = curDotIdx = 0;
            SetBackpackActive(false);
        }

        private Vector3 GetUIPosOffset(int count, int idx) {
            if (count <= 1)
                return Vector3.zero;
            float leftMostX = -(space * (count - 1) / 2f);
            float biasX = leftMostX + space * idx;
            float biasY = Mathf.Sqrt((radius * radius) - (biasX * biasX)) - radius;
            return new Vector3(biasX, biasY, 0f);
        }

        private void SetBackpackActive(bool active) {
            isActive = active;
            UICamera.gameObject.SetActive(active);
            if (active)
                ResetUIInfo();
        }

        private void FixupShowOffset() {
            if (curDotIdx - curShowOffset >= maxItemsInScreen)
                curShowOffset = curDotIdx - maxItemsInScreen + 1;
            if (curShowOffset > curDotIdx)
                curShowOffset = curDotIdx;
            if (curShowOffset + maxItemsInScreen > holdingItems.Count)
                curShowOffset = Mathf.Max(holdingItems.Count - maxItemsInScreen, 0);
        }

        public void ResetUIInfo() {
            if (curSelectIdx >= 0) {
                curDotIdx = curSelectIdx;
            } else {
                /* Keep Dot idx, but need to check list length */
                if (curDotIdx >= holdingItems.Count)
                    curDotIdx = holdingItems.Count - 1;
            }
        }

        private void HanleInput() {
            if (Input.GetKeyDown(KeyCode.I))
                SetBackpackActive(!isActive);

            if (!isActive)
                return;
            if (holdingItems.Count == 0) {
                curDotIdx = -1;
                return;
            }

            int moveRight = 0;
            if (Input.GetKeyDown(KeyCode.A))
                moveRight--;
            if (Input.GetKeyDown(KeyCode.D))
                moveRight++;
            curDotIdx = Mathf.Clamp(curDotIdx + moveRight, 0, holdingItems.Count - 1);
            FixupShowOffset();

            if (Input.GetKeyDown(KeyCode.H)) {
                if (curSelectIdx != curDotIdx) {
                    DropPick();
                    SelectCurrent();
                } else {
                    DropPick();
                }
            }

            void DropPick() {
                var pickItem = BackpackManager.Inst.player.holdingItem;
                if (pickItem != null) {
                    BackpackManager.Inst.player.Drop();
                    BackpackManager.SelectInBackpack(-1);
                    Destroy(pickItem);
                }
            }

            void SelectCurrent() {
                var pickItem = BackpackManager.CreateBackpackItem(curDotIdx);
                BackpackManager.Inst.player.Pick(pickItem);
                BackpackManager.SelectInBackpack(curDotIdx);
            }
        }

        private bool ShouldShowInUI(int idx) {
            return idx >= curShowOffset && (idx - curShowOffset) < maxItemsInScreen;
        }

        private void UpdateUIGFX() {
            int showItemCnt = Mathf.Min(holdingItems.Count, maxItemsInScreen);
            chooseUI.SetActive(ShouldShowInUI(curSelectIdx));
            chooseDot.SetActive(ShouldShowInUI(curDotIdx));
            chooseDot.transform.localPosition = GetUIPosOffset(showItemCnt, curDotIdx - curShowOffset);
            chooseUI.transform.localPosition = GetUIPosOffset(showItemCnt, curSelectIdx - curShowOffset);

            /* Update item position */
            for (int i = 0; i < holdingItems.Count; i++) {
                if (ShouldShowInUI(i)) {
                    holdingItems[i].SetActive(true);
                    Vector3 offset = GetUIPosOffset(showItemCnt, i - curShowOffset);
                    Vector3 worldOffset = offset * GetComponentInChildren<Canvas>().transform.lossyScale.x;
                    holdingItems[i].transform.position = transform.position + worldOffset;
                } else {
                    holdingItems[i].SetActive(false);
                }
            }
        }

        private void Update() {
            HanleInput();
            UpdateUIGFX();
        }
    }
}
