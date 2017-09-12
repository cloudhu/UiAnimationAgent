// <summary>
//  UiAnimationAgent
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 7/10/2017
// --------------------------------------------------------------------------------------------------------------------

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CLOUDHU.UIAnimationAgent {

	public enum eBtnAnimationType    //按钮类型枚举
{
		small,
		medium,
		big,
	};

	/// <summary>
	/// FileName: BtnAnimation.cs
	/// Author: 胡良云（CloudHu）
	/// Corporation: 
	/// Description: 基于DOTween,按钮动画控制组件
	/// DateTime: 7/10/2017
	/// </summary>
	public class BtnAnimation : MonoBehaviour {
		#region Public Variables  //公共变量区域

		[Tooltip("按钮大小类型")]
		public eBtnAnimationType btnType = eBtnAnimationType.small;
		[Tooltip("按下动画持续时间:秒")]
		public float timeDown = 0.1f;

		[Tooltip("按钮按下缩放比例")]
		public float scaleDown = 0.85f;
		[Tooltip("Alpha值")]
		public float alphaDown = 0.8f;

		public float timeDownStep1 = 0.1f;
		public float scaleDownStep1 = 1;
		public float alphaDownStep1 = 1;
		[Tooltip("松开动画持续时间:秒")]
		public float timeUp = 0.05f;

		[Tooltip("按钮松开缩放比例")]
		public float scaleUp = 1.2f;

		[Tooltip("Alpha值")]
		public float alphaUp = 1f;

		//[Tooltip("动画延迟多少秒后开始")]
		//public float delay = 0;

		[Tooltip("动画曲线类型")]
		public string easetype = "spring";

		public float timeUpStep1 = 0.1f;
		public float scaleUpStep1 = 1;
		public float alphaUpStep1 = 1;

		#endregion


		#region Private Variables   //私有变量区域
		bool m_bIfDownStep1 = false;//按下动画是否有额外步骤
		bool m_bIfUpStep1 = true;//松开动画是否有额外步骤
		Image m_imgComponent;//图片组件
		private bool m_gray = false;// 默认可用，变灰不可以再变色  
		//private bool m_init = false;
		Button m_BtnComponent;
		private enum eBtnEffectTpye {
			None,
			Light,
			Dark,
			Gray,
		}

		#endregion

		#region MonoBehaviour CallBacks //MonoBehaviour回调函数区域

		private void Awake() {
			m_imgComponent = transform.GetComponent<Image>();
			m_BtnComponent = transform.GetComponent<Button>();
		}

		// Use this for initialization
		void Start() {
			if (null == m_imgComponent) {
				return;
			}
			m_imgComponent.material = new Material(Shader.Find("UI/ImageEffectShader"));
			//m_init = true;
			EventTriggerListener.Get(gameObject).onDown = OnBtnDowm;
			EventTriggerListener.Get(gameObject).onUp = OnBtnUp;
			CSVTable table = CSVHelper.Instance().SelectFrom("uiAnimationConfig");
			if (null != table) {
				string prefix = "";
				switch (btnType) {
					case eBtnAnimationType.small:
						prefix = "small";
						break;
					case eBtnAnimationType.medium:
						prefix = "medium";
						break;
					case eBtnAnimationType.big:
						prefix = "big";
						break;
					default:
						break;
				}

				if (table.ContainsKey(prefix + "ButtonDownStep1")) {
					m_bIfDownStep1 = true;
					timeDown = float.Parse(table[prefix + "ButtonDownStep1"]["time"]);
					scaleDown = float.Parse(table[prefix + "ButtonDownStep1"]["scale"]);
					alphaDown = float.Parse(table[prefix + "ButtonDownStep1"]["alpha"]);
					timeDownStep1 = float.Parse(table[prefix + "ButtonDownEnd"]["time"]);
					scaleDownStep1 = float.Parse(table[prefix + "ButtonDownEnd"]["scale"]);
					alphaDownStep1 = float.Parse(table[prefix + "ButtonDownEnd"]["alpha"]);
				}
				else {
					timeDown = float.Parse(table[prefix + "ButtonDownEnd"]["time"]);
					scaleDown = float.Parse(table[prefix + "ButtonDownEnd"]["scale"]);
					alphaDown = float.Parse(table[prefix + "ButtonDownEnd"]["alpha"]);
				}

				if (table.ContainsKey(prefix + "ButtonReleaseStep1")) {
					//Debug.Log(table["smallButtonReleaseStep1"]["time"]);
					m_bIfUpStep1 = true;
					timeUp = float.Parse(table[prefix + "ButtonReleaseStep1"]["time"]);
					scaleUp = float.Parse(table[prefix + "ButtonReleaseStep1"]["scale"]);
					alphaUp = float.Parse(table[prefix + "ButtonReleaseStep1"]["alpha"]);
					timeUpStep1 = float.Parse(table[prefix + "ButtonReleaseEnd"]["time"]);
					scaleUpStep1 = float.Parse(table[prefix + "ButtonReleaseEnd"]["scale"]);
					alphaUpStep1 = float.Parse(table[prefix + "ButtonReleaseEnd"]["alpha"]);
				}
				else {
					timeUp = float.Parse(table[prefix + "ButtonReleaseEnd"]["time"]);
					scaleUp = float.Parse(table[prefix + "ButtonReleaseEnd"]["scale"]);
					alphaUp = float.Parse(table[prefix + "ButtonReleaseEnd"]["alpha"]);
				}
			}


		}


		#endregion

		#region Public Methods	//公共方法区域
		public void SetGray(bool bGray) {
			m_gray = bGray;
			if (bGray)
				SetShader(eBtnEffectTpye.Gray);
			else
				SetShader(eBtnEffectTpye.None);
		}

		#endregion

		#region Private Methods	//私有方法区域

		private void SetShader(eBtnEffectTpye type) {
			if (null == m_imgComponent.material)
				return;
			if (m_gray && type != eBtnEffectTpye.Gray)
				return;
			m_imgComponent.material.SetInt("_type", (int)type);
		}

		/// <summary>
		/// 按钮按下事件
		/// </summary>
		/// <param name="go"></param>
		private void OnBtnDowm(GameObject go) {
			if (go == gameObject) {
				if (null != m_BtnComponent && !m_BtnComponent.interactable) {
					SetShader(eBtnEffectTpye.Gray);
					return;
				}
				SetShader(eBtnEffectTpye.Light);
				if (m_bIfDownStep1) {
					transform.DOScale(scaleDown, timeDown).OnComplete(OnCompleteDown);
				}
				else {
					transform.DOScale(scaleDown, timeDown);
				}
				m_imgComponent.CrossFadeAlpha(alphaDown, timeDown, false);

			}
		}

		/// <summary>
		/// 按钮按下后回弹
		/// </summary>
		void OnCompleteDown() {
			m_imgComponent.CrossFadeAlpha(alphaDownStep1, timeDownStep1, false);
			transform.DOScale(scaleDownStep1, timeDownStep1);
		}

		/// <summary>
		/// 按钮释放事件
		/// </summary>
		/// <param name="go"></param>
		private void OnBtnUp(GameObject go) {
			if (go == gameObject) {
				if (null != m_BtnComponent && !m_BtnComponent.interactable) {
					SetShader(eBtnEffectTpye.Gray);
					return;
				}
				SetShader(eBtnEffectTpye.None);
				if (m_bIfUpStep1) {
					transform.DOScale(scaleUp, timeUp).OnComplete(OnCompleteUp);
				}
				else {
					transform.DOScale(scaleUp, timeUp);

				}
				m_imgComponent.CrossFadeAlpha(alphaUp, timeUp, false);

			}
		}

		/// <summary>
		/// 松开后的回弹效果
		/// </summary>
		void OnCompleteUp() {
			m_imgComponent.CrossFadeAlpha(alphaUpStep1, timeUpStep1, false);
			transform.DOScale(scaleUpStep1, timeUpStep1);
		}

		#endregion

	}
}
