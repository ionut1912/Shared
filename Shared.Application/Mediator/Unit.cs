using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Mediator;

/// <summary>
/// Represents a void type, since void is not a valid return type in C#
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
{
    public static readonly Unit Value = new();

    public int CompareTo(Unit other) => 0;

    public bool Equals(Unit other) => true;

    public override bool Equals(object? obj) => obj is Unit;

    public override int GetHashCode() => 0;

    public static bool operator ==(Unit left, Unit right) => true;

    public static bool operator !=(Unit left, Unit right) => false;

    public override string ToString() => "()";
}
