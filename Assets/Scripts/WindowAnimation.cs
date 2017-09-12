// <summary>
//  UiAnimationAgent
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 7/10/2017
// --------------------------------------------------------------------------------------------------------------------

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CLOUDHU.UIAnimationAgent {
	/// <summary>
	/// FileName: WindowAnimation.cs
	/// Author: 胡良云（CloudHu）
	/// Corporation: 
	/// Description: 基于DOTween,窗口动画控制组件
	/// DateTime: 7/10/2017
	/// </summary>
	/// 
	public enum WindowAnimationType    //按钮类型
	{
		small,
		medium,
		big
	}

	public class WindowAnimation : MonoBehaviour {

		#region Public Variables  //公共变量区域


		float alphaInit = 0;
		float scaleInit = 0;
		[Tooltip("按钮大小类型")]
		public WindowAnimationType windowType = WindowAnimationType.small;
		[Tooltip("动画持续时间:秒")]
		public float timeOpen = 0.1f;

		[Tooltip("缩放比例")]
		public float scaleOpen = 0.95f;
		[Tooltip("Alpha值")]
		public float alphaOpen = 0.8f;

		public float timeOpenStep1 = 0;
		public float alphaOpenStep1 = 0;
		public float scaleOpenStep1 = 0;

		[Tooltip("松开动画持续时间:秒")]
		public float timeClose = 0.05f;

		[Tooltip("按钮松开缩放比例")]
		public float scaleClose = 1f;

		[Tooltip("Alpha值")]
		public float alphaClose = 1f;

		//[Tooltip("动画延迟多少秒后开始")]
		//public float delay = 0;

		[Tooltip("动画曲线类型")]
		public string easetype = "linear";

		//[Tooltip("动画循环类型")]
		//public iTween.LoopType looptype = iTween.LoopType.none;

		public float timeCloseStep1 = 0;
		public float alphaCloseStep1 = 0;
		public float scaleCloseStep1 = 0;
		public delegate void CloseWindowAnimationCallBack(GameObject callback);

		public Image m_imgBG = null;
		#endregion


		#region Private Variables   //私有变量区域
		bool m_bIfOpenStep1 = false;//按下动画是否有额外步骤
		bool m_bIfCloseStep1 = false;//松开动画是否有额外步骤

		bool m_IfInitFinished = false;  //是否完成动画配置初始化
		CloseWindowAnimationCallBack closeWindowAnimationCallBackTmp;
		CanvasGroup m_pcCavasGroup;

		float m_InitAlpha = float.NaN;

		#endregion

		#region MonoBehaviour CallBacks //MonoBehaviour回调函数区域


		// Use this for initialization
		void Awake() {
			m_pcCavasGroup = GetComponent<CanvasGroup>();
			if (null == m_pcCavasGroup) {
				m_pcCavasGroup = gameObject.AddComponent<CanvasGroup>();
			}

			if (m_imgBG == null) {
				m_imgBG = this.GetComponent<Image>();
				if (m_imgBG == null) {
					Image[] listImages = this.transform.GetComponentsInChildren<Image>();
					for (int i = 0; i < listImages.Length; i++) {
						if (listImages[i].sprite != null && listImages[i].sprite.name.Contains("mengban")) {
							m_imgBG = listImages[i];
							break;
						}
					}
				}
			}
		}



		private void Start() {
			CSVTable table;//配置表
			table = CSVHelper.Instance().SelectFrom("uiAnimationConfig");
			if (null != table) {
				string prefix = "";//窗口大小前缀
				switch (windowType) {
					case WindowAnimationType.small:
						prefix = "small";
						break;
					case WindowAnimationType.medium:
						prefix = "medium";
						break;
					case WindowAnimationType.big:
						prefix = "big";
						break;
					default:
						break;
				}
				//Debug.Log(prefix + "WindowOpenStart");
				//初始化值
				alphaInit = float.Parse(table[prefix + "WindowOpenStart"]["alpha"]);

				scaleInit = float.Parse(table[prefix + "WindowOpenStart"]["scale"]);
				if (table.ContainsKey(prefix + "WindowOpenStep1")) {
					m_bIfOpenStep1 = true;
					timeOpen = float.Parse(table[prefix + "WindowOpenStep1"]["time"]);
					scaleOpen = float.Parse(table[prefix + "WindowOpenStep1"]["scale"]);
					alphaOpen = float.Parse(table[prefix + "WindowOpenStep1"]["alpha"]);
					alphaOpenStep1 = float.Parse(table[prefix + "WindowOpenEnd"]["alpha"]);
					timeOpenStep1 = float.Parse(table[prefix + "WindowOpenEnd"]["time"]);
					scaleOpenStep1 = float.Parse(table[prefix + "WindowOpenEnd"]["scale"]);

				}
				else {
					timeOpen = float.Parse(table[prefix + "WindowOpenEnd"]["time"]);
					scaleOpen = float.Parse(table[prefix + "WindowOpenEnd"]["scale"]);
					alphaOpen = float.Parse(table[prefix + "WindowOpenEnd"]["alpha"]);
				}

				if (table.ContainsKey(prefix + "WindowCloseStep1")) {
					//Debug.Log(table["smallWindowCloseStep1"]["time"]);
					m_bIfCloseStep1 = true;
					timeClose = float.Parse(table[prefix + "WindowCloseStep1"]["time"]);
					scaleClose = float.Parse(table[prefix + "WindowCloseStep1"]["scale"]);
					alphaClose = float.Parse(table[prefix + "WindowCloseStep1"]["alpha"]);
					alphaCloseStep1 = float.Parse(table[prefix + "WindowCloseEnd"]["alpha"]);
					timeCloseStep1 = float.Parse(table[prefix + "WindowCloseEnd"]["time"]);
					scaleCloseStep1 = float.Parse(table[prefix + "WindowCloseEnd"]["scale"]);
				}
				else {
					timeClose = float.Parse(table[prefix + "WindowCloseEnd"]["time"]);
					scaleClose = float.Parse(table[prefix + "WindowCloseEnd"]["scale"]);
					alphaClose = float.Parse(table[prefix + "WindowCloseEnd"]["alpha"]);
				}
				//Debug.Log(gameObject.name + "=>prefix:"+ prefix + "  >>>  " + "timeOpen:" + timeOpen + "+ scaleOpen:" + scaleOpen + "+  alphaOpen:" + alphaOpen + "+  alphaOpenStep1:" + alphaOpenStep1 + "+  timeOpenStep1: " + timeOpenStep1 + "+  timeOpenStep1" + timeOpenStep1 + "+  scaleOpenStep1:" + scaleOpenStep1 + "+  timeSinceLevelLoad: " + Time.timeSinceLevelLoad);
				m_IfInitFinished = true;
				OnWindowOpen();
				Debug.Log("读取动画配置表成功" + prefix);
			}
			table = null;
		}

		private void OnEnable() {
			if (m_imgBG != null) {
				if (float.IsNaN(m_InitAlpha)) {
					m_InitAlpha = m_imgBG.color.a;
				}
				Color _color = new Color(m_imgBG.color.r, m_imgBG.color.g, m_imgBG.color.b, m_InitAlpha);
				m_imgBG.color = _color;
			}

			if (m_IfInitFinished) {
				OnWindowOpen();
			}
		}

		#endregion

		#region Public Methods	//公共方法区域

		/// <summary>
		/// 关闭窗口
		/// </summary>
		public void OnWindowClose(CloseWindowAnimationCallBack windowAnimationCallBack) {
			if (!m_IfInitFinished) {
				//Debug.Log(m_IfInitFinished);
				windowAnimationCallBack.Invoke(gameObject);
				return;
			}

			if (m_imgBG != null) {
				Color _color = new Color(m_imgBG.color.r, m_imgBG.color.g, m_imgBG.color.b, 0);
				m_imgBG.color = _color;
			}

			if (m_bIfCloseStep1) {
				closeWindowAnimationCallBackTmp = windowAnimationCallBack;
				transform.DOScale(scaleClose, timeClose).OnComplete(OnCompleteClose);
			}
			m_pcCavasGroup.DOFade(alphaClose, timeClose);
			if (!m_bIfCloseStep1 && null != windowAnimationCallBack) {
				windowAnimationCallBack.Invoke(gameObject);
			}
		}
		#endregion

		#region Private Methods	//私有方法区域

		/// <summary>
		/// 打开窗口
		/// </summary>
		private void OnWindowOpen() {
			if (!m_IfInitFinished) {
				OnCompleteWindowAdjust();
				return;
			}
			m_pcCavasGroup.alpha = alphaInit;
			anchorAjust();
			transform.localScale = Vector3.one * scaleInit;
			
			if (m_bIfOpenStep1) {
				transform.DOScale(scaleOpen, timeOpen).OnComplete(OnCompleteWindowOpen);
			}
			else {
				transform.DOScale(scaleOpen, timeOpen).OnComplete(OnCompleteWindowAdjust);
			}
			m_pcCavasGroup.DOFade(alphaOpen, timeOpen);
		}

		/// <summary>
		/// 窗口动画完成后进行自适应
		/// </summary>
		void OnCompleteWindowAdjust() {
			anchorAjust();
		}

		void anchorAjust() {
			RectTransform rect = GetComponent<UnityEngine.RectTransform>();
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
		}

		void OnCompleteWindowOpen() {
			if (!m_IfInitFinished) {
				return;
			}
			transform.DOScale(scaleOpenStep1, timeOpenStep1).OnComplete(OnCompleteWindowAdjust);
			m_pcCavasGroup.DOFade(alphaOpenStep1, timeOpenStep1);
		}

		/// <summary>
		/// 回弹效果
		/// </summary>
		void OnCompleteClose() {
			if (!m_IfInitFinished) {
				return;
			}
			transform.DOScale(scaleCloseStep1, scaleCloseStep1).OnComplete(CloseWindowCallbackMethod);
			m_pcCavasGroup.DOFade(alphaCloseStep1, scaleCloseStep1);

		}

		/// <summary>
		/// 关闭窗口回调
		/// </summary>
		private void CloseWindowCallbackMethod() {
			if (null != closeWindowAnimationCallBackTmp) {
				closeWindowAnimationCallBackTmp.Invoke(gameObject);
			}
		}

		#endregion
	}
}
