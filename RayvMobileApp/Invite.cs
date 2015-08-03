using System;
using System.Linq;
using System.Collections.Generic;

namespace RayvMobileApp
{
	public class Invite
	{
		public string inviter { get; set; }

		public string invitee{ get; set; }

		public string name{ get; set; }

		public bool accepted{ get; set; }

		public DateTime when { get; set; }

		public static bool AcceptInvite (string from)
		{
			var param = new Dictionary<string, string> {
				{ "from_id",from }
			};
			bool success = restConnection.Instance.post ("/api/friends/accept", param) == "OK";
			if (success) {
				var invite = Persist.Instance.InvitationsIn.Where (i => i.inviter == from).FirstOrDefault ();
				Persist.Instance.Acceptances.Add (invite);
				Persist.Instance.InvitationsIn.Remove (invite);
			}
			return success;
		}

		public static bool RejectInvite (string from)
		{
			var param = new Dictionary<string, string> {
				{ "from_id",from }
			};
			bool success = restConnection.Instance.post ("/api/friends/reject", param) == "OK";
			if (success) {
				var invite = Persist.Instance.InvitationsIn.Where (i => i.inviter == from).FirstOrDefault ();
				Persist.Instance.InvitationsIn.Remove (invite);
			}
			return success;
		}

		public static bool DismissAcceptance (string from)
		{
			var param = new Dictionary<string, string> {
				{ "from_id",from }
			};
			bool success = restConnection.Instance.post ("/api/friends/reject", param) == "OK";
			if (success) {
				var invite = Persist.Instance.Acceptances.Where (i => i.invitee == from).FirstOrDefault ();
				Persist.Instance.Acceptances.Remove (invite);
			}
			return success;
		}

		public Invite ()
		{
		}
	}
}

