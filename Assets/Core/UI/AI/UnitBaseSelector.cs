using UnityEngine;
using UnityEngine.UI;

public class UnitBaseSelector : MonoBehaviour
{
    public Button SelectButton => m_selectButton;

    [SerializeField] private GameTeamData m_gameTeamData = default;
    [SerializeField] private Button m_selectButton = default;
    [SerializeField] private Image m_selectImage = default;

    public void UpdateDisplay(Entity localPlayer, BaseController controller)
    {
        if (controller.TeamOwner == "")
        {
            m_selectImage.color = m_gameTeamData.DefaultTeamColour;
            ColorBlock cb = m_selectButton.colors;
            cb.disabledColor = m_gameTeamData.DefaultTeamColour;
            m_selectButton.colors = cb;
        }
        else
        {
            m_selectImage.color = m_gameTeamData.GetTeamData(controller.TeamOwner).Colour;
            ColorBlock cb = m_selectButton.colors;
            cb.disabledColor = m_gameTeamData.GetTeamData(controller.TeamOwner).Colour;
            m_selectButton.colors = cb;
        }
    }

    public void SetAnchorPosition(Vector3 screenPos)
    {
        screenPos.x /= 1920.0f;
        screenPos.y /= 1080.0f;

        GetComponent<RectTransform>().anchorMin = screenPos;
        GetComponent<RectTransform>().anchorMax = screenPos;
        GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
    }
}
