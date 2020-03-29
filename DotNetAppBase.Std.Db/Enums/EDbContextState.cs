namespace DotNetAppBase.Std.Db.Enums
{
	/// <summary>
	///     Determina o estado de um ISqlContext
	/// </summary>
	public enum EDbContextState
	{
		/// <summary>
		///     Objeto de acesso n�o est� em um contexto de transa��o
		/// </summary>
		OutTransaction,

		/// <summary>
		///     Objeto de acesso est� em um contexto de transa��o
		/// </summary>
		InTransaction,

		/// <summary>
		///     Transa��o foi confirmada (Commit)
		/// </summary>
		Confirmed,

		/// <summary>
		///     Transa��o foi cancelada (Rollback)
		/// </summary>
		Cancelled,

		/// <summary>
		///     Quando o contexto liberou os recursos utilizados, tornando-se indispon�vel
		/// </summary>
		Disposed
	}
}