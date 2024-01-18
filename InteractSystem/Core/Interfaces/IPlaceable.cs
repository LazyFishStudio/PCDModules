using UnityEngine;

namespace InteractSystem
{
	/// <summary>
	/// Usually it is also IPickable.
	/// </summary>
    public interface IPlaceable
	{
		public IPlaceSlot attachedPlace { get; set; }
		/// <summary>
		/// ** MUST ** call slot.CheckAcceptItem before calling this method.
		/// </summary>
		public bool PlacedTo(InteractComp interactor, IPlaceSlot slot);
		public bool RemovedFrom(InteractComp interactor);
	}
}
