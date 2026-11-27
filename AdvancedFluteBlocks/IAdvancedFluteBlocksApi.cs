namespace AdvancedFluteBlocks
{
	public interface IAdvancedFluteBlocksApi
	{
		/// <summary>
		/// Gets the flute block tone based on the provided index.
		/// </summary>
		/// <param name="index">The index of the tone.</param>
		/// <returns>The tone at the specified index, or null if the index is out of range.</returns>
		string GetFluteBlockToneFromIndex(int index);
	}
}
