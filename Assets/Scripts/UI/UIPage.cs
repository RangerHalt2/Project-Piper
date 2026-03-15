// Dummy Script used to mark something as a page
using UnityEngine;

public class UIPage : MonoBehaviour
{
    [Header("Page Behavior")]
    [Tooltip("If true, pause input is allowed while this page is active.")]
    [SerializeField] private bool allowPauseOnThisPage = false;

    public bool AllowPauseOnThisPage => allowPauseOnThisPage;
}
