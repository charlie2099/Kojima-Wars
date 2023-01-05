using UnityEngine;

public class InitialBaseController : BaseController
{
    public string InitialBaseOwner => m_baseOwner;

    [Header("Initial Base Set Up")]
    [SerializeField] private string m_baseOwner = default;
    [SerializeField] private GameObject m_laserObject = null;
    [SerializeField] private GameTeamData m_teamDataSO = null;

    protected override void Awake()
    {
        base.Awake();
        ChangeTeamOwner(m_baseOwner);
        m_laserObject.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Owner"), m_teamDataSO.GetTeamData(m_baseOwner).Colour);
    }
}