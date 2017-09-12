using CLOUDHU.UIAnimationAgent;//引用动画组件的命名空间
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 测试脚本
/// </summary>
public class Test : MonoBehaviour {

	public GameObject m_widowPrefab;//窗口预设

	Button _btnOpen,_btnClose;//打开和关闭按钮

	// Use this for initialization
	void Start () {
		_btnOpen = transform.FindChild("BtnOpen").GetComponent<Button>();
		_btnClose = transform.FindChild("BtnClose").GetComponent<Button>();
		//为按钮添加按钮动画组件
		_btnOpen.gameObject.AddComponent<BtnAnimation>().btnType = eBtnAnimationType.medium;
		_btnClose.gameObject.AddComponent<BtnAnimation>().btnType = eBtnAnimationType.small;
		//为按钮添加点击事件监听
		_btnOpen.onClick.AddListener(OnBtnOpenClick);
		_btnClose.onClick.AddListener(OnBtnCloseClick);

	}

	/// <summary>
	/// 点击打开窗口按钮
	/// </summary>
	private void OnBtnOpenClick() {
		if (null!= m_widowPrefab) {
			GameObject newWindow = GameObject.Instantiate(m_widowPrefab);
			//每次打开一种颜色的窗口
			Image bg = newWindow.GetComponent<Image>();
			bg.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			newWindow.transform.SetParent(transform.parent, false);
			//为新窗口添加窗口动画组件并设置窗口类型
			newWindow.AddComponent<WindowAnimation>().windowType = WindowAnimationType.big;
			newWindow.AddComponent<Test>().m_widowPrefab=m_widowPrefab;
			newWindow.transform.FindChild("BtnClose").gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// 点击关闭按钮
	/// </summary>
	private void OnBtnCloseClick() {
		WindowAnimation wa = GetComponent<WindowAnimation>();
		if (null!=wa) { //如果窗口上有WindowAnimation组件,那么先执行动画,再执行Destroy(gameObject);
						//关闭窗口回调
			wa.OnWindowClose((close) => {
				Destroy(gameObject);
			});
		}
		else {
			Destroy(gameObject);
		}
	}

	// 当 MonoBehaviour 将被销毁时调用此函数
	private void OnDestroy() {
		_btnOpen.onClick.RemoveListener(OnBtnOpenClick);
		_btnClose.onClick.RemoveListener(OnBtnCloseClick);
	}


}
