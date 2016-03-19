namespace WpfRealSenseHands.Controls
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Text;

  class HandAlertManager
  {
    [Flags]
    public enum HandStatus
    {
      Ok =
        PXCMHandData.AlertType.ALERT_HAND_CALIBRATED |
        PXCMHandData.AlertType.ALERT_HAND_DETECTED |
        PXCMHandData.AlertType.ALERT_HAND_INSIDE_BORDERS |
        PXCMHandData.AlertType.ALERT_HAND_TRACKED,
      NotTracked =
        PXCMHandData.AlertType.ALERT_HAND_CALIBRATED |
        PXCMHandData.AlertType.ALERT_HAND_DETECTED |
        PXCMHandData.AlertType.ALERT_HAND_INSIDE_BORDERS,
      NotCalibrated =
        PXCMHandData.AlertType.ALERT_HAND_TRACKED |
        PXCMHandData.AlertType.ALERT_HAND_DETECTED |
        PXCMHandData.AlertType.ALERT_HAND_INSIDE_BORDERS,
      NotInsideBorders =
        PXCMHandData.AlertType.ALERT_HAND_CALIBRATED |
        PXCMHandData.AlertType.ALERT_HAND_DETECTED |
        PXCMHandData.AlertType.ALERT_HAND_TRACKED
    }
    public HandAlertManager(PXCMHandConfiguration handConfig)
    {
      this.statusValues = new Dictionary<int, HandStatus>();
      handConfig.EnableAllAlerts();
      handConfig.SubscribeAlert(this.OnAlert);
    }
    void OnAlert(PXCMHandData.AlertData alertData)
    {
      // NB: ignoring the low confidence state as I'm not sure what to do with
      // it because there is no 'high' confidence state.
      // Also, assumed that if I get an out of bottom/left/right/top then I
      // will also get an OUT_OF_BORDERS so I'm ignoring the 4 explicit states.
      // Also assuming that a HAND_TOO_CLOSE/FAR is followed by an INSIDE_BORDERS
      // as that seems to happen.
      HandStatus handStatus = HandStatus.Ok;
      bool alertTypeToAdd = false;

      if (this.statusValues.ContainsKey(alertData.handId))
      {
        handStatus = this.statusValues[alertData.handId];
      }

      switch (alertData.label)
      {
        case PXCMHandData.AlertType.ALERT_HAND_CALIBRATED:
        case PXCMHandData.AlertType.ALERT_HAND_DETECTED:
        case PXCMHandData.AlertType.ALERT_HAND_INSIDE_BORDERS:
        case PXCMHandData.AlertType.ALERT_HAND_TRACKED:
          handStatus |= (HandStatus)alertData.label;
          alertTypeToAdd = true;
          break;
        case PXCMHandData.AlertType.ALERT_HAND_NOT_CALIBRATED:
          handStatus &= (HandStatus)~PXCMHandData.AlertType.ALERT_HAND_CALIBRATED;
          break;
        case PXCMHandData.AlertType.ALERT_HAND_NOT_DETECTED:
          handStatus &= (HandStatus)~PXCMHandData.AlertType.ALERT_HAND_DETECTED;
          break;
        case PXCMHandData.AlertType.ALERT_HAND_NOT_TRACKED:
          handStatus &= (HandStatus)~PXCMHandData.AlertType.ALERT_HAND_TRACKED;
          break;
        case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_BORDERS:
        case PXCMHandData.AlertType.ALERT_HAND_TOO_CLOSE:
        case PXCMHandData.AlertType.ALERT_HAND_TOO_FAR:
        case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_BOTTOM_BORDER:
        case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_LEFT_BORDER:
        case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_RIGHT_BORDER:
        case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_TOP_BORDER:
          handStatus &= (HandStatus)~PXCMHandData.AlertType.ALERT_HAND_INSIDE_BORDERS;
          break;
        default:
          break;
      }
      if ((handStatus & (HandStatus)PXCMHandData.AlertType.ALERT_HAND_DETECTED) == 0)
      {
        // remove a value if we've been told that it's no longer detected.
        this.statusValues.Remove(alertData.handId);
      }
      else if (this.statusValues.ContainsKey(alertData.handId) || alertTypeToAdd)
      {
        // store any value into an existing slot but don't add a new slot
        // unless there's a good reason (i.e. not if we are receiving an
        // update to a hand that's been removed already).
        this.statusValues[alertData.handId] = handStatus;
      }
    }
    public IDictionary<int, HandStatus> GetHandsInfo()
    {
      return (this.statusValues);
    }
    Dictionary<int, HandStatus> statusValues;
  }
}