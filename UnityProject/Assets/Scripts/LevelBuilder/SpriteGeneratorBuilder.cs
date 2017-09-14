﻿#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MapFeatures;

/* A Builder pattern that is used to build Generators. A generators 
 * is a method that generates an object using a Manager. For more 
 * details see Editor/LevelGenerator/Generator and the managers 
 * from Scripts/MapFeatures.
 * 
 * The SpriteGeneratorBuilder *has* to be attached to an object.
 * 
 * To keep the state of the scene once the scene is close, we *need to keep 
 * a serializable object* that encasulates the state that describes how an 
 * object should be generated.
 * 
 * The most of design is kept generic-less as Unity does not support adding 
 * generic components.
 */

[ExecuteInEditMode]
public class SpriteGeneratorBuilder : MonoBehaviour
{
	[SerializeField] private float x;
	[SerializeField] private float y;
	[SerializeField] private int width;
	[SerializeField] private int height;
	[SerializeField] private string path;
	[SerializeField] private bool hasLights;
	[SerializeField] private LightData lights;

	// The type is kept a type name that also refences also the assembly in which the
	// type is found. Using a type is harder in this context as the Type may not be
	// loaded inside the Assembly-CSharp(as the types are in Assembly-CSharp-Editor).
	[SerializeField] private string typeName;

	public SpriteGeneratorBuilder CreateBuilder(Type type)
	{
		this.x = 0.0f;
		this.y = 0.0f;
		this.width = 0;
		this.height = 0;
		this.path = null;
		this.typeName = type == null ? null : Convert.ToString(type.AssemblyQualifiedName);
		this.hasLights = false;

		return this;
	}

	public SpriteGeneratorBuilder ByBuilder(SpriteGeneratorBuilder builder)
	{
		this.x = builder.x;
		this.y = builder.y;
		this.width = builder.width;
		this.height = builder.height;
		this.path = builder.path;
		this.typeName = builder.typeName;
		this.hasLights = builder.hasLights;
		this.lights = builder.lights;

		return this;
	}

	public IGenerator Build()
	{
		Type type = Type.GetType(typeName);

		// All the generators have only this 2 kinds of contructors.
		// We create the instances using Reflection rather than using 
		// generics as we do not know the type at runtime. For refence look 
		// at Activator documentation.
		// The only 2 constructors for the classes present so far inside the 
		// Generator module have only the constructors (float, float) and (float, float, string).
		if (path == null) 
			return (IGenerator) Activator.CreateInstance(type, new object[] {x, y});
		else 
			return (IGenerator) Activator.CreateInstance(type, new object[] {x, y, GetSprite()});
	}

	public SpriteGeneratorBuilder ByCoord(float x, float y)
	{
		this.x = x;
		this.y = y;

		return this;
	}

	public SpriteGeneratorBuilder ByLightCoord(float x, float y)
	{
		this.lights.x = x;
		this.lights.y = y;

		return this;
	}

	// This should be called before ByWidth, ByHeight
	public SpriteGeneratorBuilder ByPath(string path)
	{
		this.path = path;

		return this;
	}

	public SpriteGeneratorBuilder ByWidth(int width)
	{
		this.width = width;
		if (height == 0) 
			height = width; 
		
		return this;
	}

	public SpriteGeneratorBuilder ByHeight(int height)
	{
		this.height = height;
		if (width == 0) 
			width = height; 
		
		return this;
	}

	public SpriteGeneratorBuilder ByLightData(LightData data)
	{
		this.lights = data;
		this.hasLights = true;

		return this;
	}

	private string GetLights()
	{
		if (!hasLights) 
			return "";

		return lights.ToJson () + ",";
	}

	private string GetSprite()
	{
		// TODO: The type should not be exported from here, to refactor.
		string unqualifiedTypeName = typeName.Split(',')[0].Split('.')[1];

		return @"
			""code"" : """ + unqualifiedTypeName  + @""",
			""sprite"" : {
				""width"" : " + width.ToString() + @",  
				""height"" : " + height.ToString() + @",
				" + GetLights() + @" 
				""path"" : """ + path + @""",
		}";
	}
}
