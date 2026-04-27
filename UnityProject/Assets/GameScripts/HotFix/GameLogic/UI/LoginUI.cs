using System.Net.WebSockets;
using GameConfig;
using GameConfig.item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, location : "LoginUI")]
	public partial class LoginUI
	{
		#region 事件

		private partial void OnClickLoginBtn()
		{
			Item itemConfig = ConfigSystem.Instance.Tables.TbItem.Get(10001);
			Debug.LogError($"item: {itemConfig.Name}");
			
			myTestTable myTestConfig = ConfigSystem.Instance.Tables.TbmyTestTable.Get(10002);
			Debug.LogError($"myTestTable: {myTestConfig.Name}");
			
			var myTestConfigList = ConfigSystem.Instance.Tables.TbmyTestTable.DataList;
			var myTestConfigMap = ConfigSystem.Instance.Tables.TbmyTestTable.DataMap;		
			
			m_imgTest.SetSprite("common_xanjian1");
			var versiona = GameModule.Resource.InternalResourceVersion;
			Debug.LogError(versiona);
		}

		#endregion

		protected override void RegisterEvent()
		{
			base.RegisterEvent();
		}

		protected override void OnCreate()
		{
			base.OnCreate();
		}
		
		protected override void OnRefresh()
		{
			base.OnRefresh();
		}

	

	}
}


