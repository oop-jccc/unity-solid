using UnityEngine;

namespace DesignPatterns.DIP
{
    /// <summary>
    /// A Switch component that can toggle the state of an ISwitchable client. This class demonstrates
    /// the Dependency Inversion Principle by depending on an abstraction (ISwitchable) rather than concrete implementations.
    /// </summary>
    public class Switch : MonoBehaviour
    {
        // Unity's serialization system does not directly support interfaces. Work around this limitation
        // by using a serialized reference to a MonoBehaviour that implements ISwitchable.

        [SerializeField] private MonoBehaviour m_ClientBehavior;
        private ISwitchable m_Client => m_ClientBehavior as ISwitchable;

        void Awake()
        {
            if (m_ClientBehavior == null)
            {
                Debug.LogError($"ClientBehavior not assigned on Switch '{gameObject.name}'. Disabling.", this);
                enabled = false;
                return;
            }

            if (m_ClientBehavior is ISwitchable) return;

            Debug.LogError($"ClientBehavior on Switch '{gameObject.name}' does not implement ISwitchable. Disabling.",
                this);
            enabled = false;
        }

        public void Toggle()
        {
            if (!enabled)
            {
                return;
            }

            if (m_Client.IsActive)
            {
                m_Client.Deactivate();
            }
            else
            {
                m_Client.Activate();
            }
        }

        /// <summary>
        /// Validates that the assigned client behavior implements the ISwitchable interface.
        /// Logs an error if it doesn't. This is useful for editor-time validation.
        /// </summary>
        void OnValidate()
        {
            // This check is for editor-time feedback.
            // It helps catch incorrect assignments as they happen in the Inspector.
            if (m_ClientBehavior is not null and not ISwitchable)
            {
                Debug.LogWarning(
                    $"Assigned ClientBehavior on '{gameObject.name}' must implement ISwitchable.", this);
            }
        }
    }
}