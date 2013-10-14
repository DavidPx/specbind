﻿// <copyright file="IActionRepository.cs">
//    Copyright © 2013 Dan Piessens  All rights reserved.
// </copyright>

namespace SpecBind.ActionPipeline
{
	using System.Collections.Generic;

	/// <summary>
	/// Contains a cache of available actions, pre-actions and post-actions.
	/// </summary>
	public interface IActionRepository
	{
		/// <summary>
		/// Gets the post-execute actions.
		/// </summary>
		/// <returns>An enumerable collection of actions.</returns>
		IEnumerable<IPostAction> GetPostActions();

		/// <summary>
		/// Gets the pre-execute actions.
		/// </summary>
		/// <returns>An enumerable collection of actions.</returns>
		IEnumerable<IPreAction> GetPreActions();

		/// <summary>
		/// Gets the locator actions.
		/// </summary>
		/// <returns>An enumerable collection of actions.</returns>
		IEnumerable<ILocatorAction> GetLocatorActions();
	}
}