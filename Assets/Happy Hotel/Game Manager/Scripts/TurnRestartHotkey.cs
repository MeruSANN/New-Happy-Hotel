using UnityEngine;
using HappyHotel.TurnRestart;

namespace HappyHotel.GameManager
{
	// 临时热键：R 键触发恢复（仅静止且玩家回合有效）
	public class TurnRestartHotkey : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				if (TurnRestartService.Instance != null && TurnRestartService.Instance.CanRestartNow())
				{
					TurnRestartService.Instance.RestoreSnapshot();
				}
			}
		}
	}
}

