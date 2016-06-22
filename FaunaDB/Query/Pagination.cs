﻿using FaunaDB.Query;
using FaunaDB.Types;
using FaunaDB.Utils;
using System;

namespace FaunaDB
{
    /// <summary>
    /// A single pagination result.
    /// See <c>paginate</c> in the <see href="https://faunadb.com/documentation/queries#read_functions">docs</see>. 
    /// </summary>
    public struct Pagination : IEquatable<Pagination>
    {
        /// <summary>
        /// Pagination results. What these are depends on the query.
        /// </summary>
        public ArrayV Data { get; }
        /// <summary>
        /// Nullable cursor to the previous page.
        /// </summary>
        public Cursor? Before { get; }
        /// <summary>
        /// Nullable cursor to the next page.
        /// </summary>
        public Cursor? After { get; }

        public Pagination(ArrayV data, Cursor? before = null, Cursor? after = null)
        {
            Data = data;
            Before = before;
            After = after;
        }

        /// <summary>
        /// Use this on a value that you know represents a Page.
        /// </summary>
        public static explicit operator Pagination(Expr v)
        {
            var obj = (ObjectV) v;
            return new Pagination((ArrayV) obj["data"], GetCursor(obj, "before"), GetCursor(obj, "after"));
        }

        static Cursor? GetCursor(ObjectV obj, string direction)
        {
            var value = obj.GetOrNull(direction);
            return value == null ? null : (Cursor?) new Cursor((ArrayV) value);
        }

        public static implicit operator Expr(Pagination p) =>
            ObjectV.With("data", p.Data, "before", p.Before?.Value, "after", p.After?.Value);

        #region boilerplate
        public override bool Equals(object obj) =>
            obj is Pagination && Equals((Pagination) obj);

        public bool Equals(Pagination p) =>
            p.Data == Data && p.Before == Before && p.After == After;

        public static bool operator==(Pagination a, Pagination b) =>
            object.Equals(a, b);

        public static bool operator!=(Pagination a, Pagination b) =>
            !object.Equals(a, b);

        public override int GetHashCode() =>
            HashUtil.Hash(Data, Before, After);

        public override string ToString() =>
            $"Page({Data}, Before: {Before}, After: {After})";
        #endregion
    }

    /// <summary>
    /// An opaque value enabling you to get a previous/next page.
    /// </summary>
    public struct Cursor : IEquatable<Cursor>
    {
        /// <summary>
        /// Cursor value. You usually won't need to access this.
        /// </summary>
        public ArrayV Value { get; }

        public Cursor(ArrayV value)
        {
            if (value == null)
                throw new NullReferenceException();
            Value = value;
        }

        public static implicit operator Expr(Cursor c) =>
            c.Value;

        public static implicit operator Value(Cursor c) =>
            c.Value;

        #region boilerplate
        public override bool Equals(object obj) =>
            obj is Cursor && Equals((Cursor) obj);

        public bool Equals(Cursor c) =>
            Value == c.Value;

        public static bool operator ==(Cursor a, Cursor b) =>
            a.Equals(b);

        public static bool operator !=(Cursor a, Cursor b) =>
            !(a == b);

        public override int GetHashCode() =>
            Value.GetHashCode();

        public override string ToString() =>
            $"Cursor({Value})";
        #endregion
    }
}