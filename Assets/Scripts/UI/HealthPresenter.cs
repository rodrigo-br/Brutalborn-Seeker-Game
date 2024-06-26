using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthPresenter : MonoBehaviour
{
    [SerializeField] private Image _healthBar;
    [SerializeField] private Health _health;
    [SerializeField] private TextMeshProUGUI _healthValuesText;
    [SerializeField] private float _barSpeed = 4f;
    private Coroutine _healthChangeCoroutine;

    private void OnEnable()
    {
        if (_health != null)
        {
            _health.OnHealthChange += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnHealthChange -= UpdateUI;
        }
    }

    private void UpdateUI(Health sender)
    {
        if (_healthChangeCoroutine != null)
        {
            StopCoroutine(_healthChangeCoroutine);
        }
        _healthValuesText.text = $"{sender.CurrentHealth} / {sender.MaxHealth}";
        _healthChangeCoroutine = StartCoroutine(HealthChangeRoutine(sender.CurrentHealth / (float)sender.MaxHealth));
    }

    private IEnumerator HealthChangeRoutine(float target)
    {
        int iterations = 0;

        while (_healthBar.fillAmount != target && iterations < 1000)
        {
            _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, target, Time.deltaTime * _barSpeed);
            iterations++;
            yield return null;
        }

        _healthBar.fillAmount = target;
        _healthChangeCoroutine = null;
        yield return null;
    }
}
