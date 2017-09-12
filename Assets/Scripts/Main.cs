using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CLOUDHU.UIAnimationAgent {
	public class Main : MonoBehaviour {

		// Use this for initialization
		void Start() {

			//0.初始化DOTween动画组件
			DG.Tweening.DOTween.Init(true, true, DG.Tweening.LogBehaviour.ErrorsOnly).SetCapacity(200, 10);
			//1.读取Excel动画配置表
			CSVHelper.Instance().ReadCSVFile("uiAnimationConfig", (table) => {
				Debug.Log("读取动画配置表成功!");
			});
			//2.给按钮添加点击事件监听
			GetComponent<Button>().onClick.AddListener(OnButtonClick);
		}

		private void OnButtonClick() {
			Debug.Log("加载UI动画组件测试场景");
			SceneManager.LoadScene("Test");
		}

		// 当 MonoBehaviour 将被销毁时调用此函数
		private void OnDestroy() {
			GetComponent<Button>().onClick.RemoveListener(OnButtonClick);
		}

	}
}

