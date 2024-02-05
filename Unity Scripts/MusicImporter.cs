using NAudio.Wave;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;
using Debug = UnityEngine.Debug;
using NAudio.Wave.SampleProviders;

[ScriptedImporter(1, "mus")]
public class MusicImporter : ScriptedImporter
{
	[InspectorButton(nameof(OnButtonClicked))]
	public bool ExtractMusic;

	public override void OnImportAsset(AssetImportContext ctx)
	{
	}

	private void OnButtonClicked()
	{
		var parentFolder = Directory.GetParent(assetPath).ToString();
		var filename = Path.GetFileNameWithoutExtension(assetPath);
		var msbFile = Path.Combine(parentFolder, $"{filename}.msb");
		var mpfFile = Path.Combine(parentFolder, $"{filename}.mpf");

		if (!File.Exists(mpfFile))
		{
			var bytes = File.ReadAllBytes(msbFile);
			var newBytes = bytes.Skip(80).ToArray();
			File.WriteAllBytes(mpfFile, newBytes);
		}

		var streamsPath = Path.Combine(parentFolder, $"streams~");

		Directory.CreateDirectory(streamsPath);

		var output = Path.Combine(streamsPath, $"{filename}_?04s.wav");
		var args = $"-S -l 0 -f 0 -o \"{output}\" \"{mpfFile}\"";
		var proc = Process.Start($"{Application.dataPath}/Editor/vgmstream-win64/vgmstream-cli", args);
		proc.WaitForExit();

		var files = Directory.GetFiles(streamsPath);
		Concatenate(Path.Combine(parentFolder, $"{filename}.wav"), files);

		foreach (var item in files)
		{
			File.Delete(item);
		}

		SplitIntoLayers(Path.Combine(parentFolder, $"{filename}.wav"));
	}

	public void Concatenate(string outputFile, string[] sourceFiles)
	{
		var readerArray = new AudioFileReader[sourceFiles.Length];
		for (int i = 0; i < sourceFiles.Length; i++)
		{
			readerArray[i] = new AudioFileReader(sourceFiles[i]);
		}

		var concatenated = new ConcatenatingSampleProvider(readerArray);
		WaveFileWriter.CreateWaveFile16(outputFile, concatenated);

		foreach (var item in readerArray)
		{
			item.Dispose();
		}
	}

	public void SplitIntoLayers(string inputFile)
	{
		var parentFolder = Path.GetDirectoryName(inputFile);
		var filename = Path.GetFileNameWithoutExtension(inputFile);

		var reader = new WaveFileReader(inputFile);
		var writers = new WaveFileWriter[3];
		for (var n = 0; n < writers.Length; n++)
		{
			var format = new WaveFormat(reader.WaveFormat.SampleRate, 16, 2);
			writers[n] = new WaveFileWriter($"{parentFolder}/{filename}_{n}.wav", format);
		}

		float[] buffer;
		while ((buffer = reader.ReadNextSampleFrame())?.Length > 0)
		{
			for (var i = 0; i < buffer.Length; i += 2)
			{
				writers[i / 2].WriteSample(buffer[i]);
				writers[i / 2].WriteSample(buffer[i + 1]);
			}
		}

		for (var n = 0; n < writers.Length; n++)
		{
			writers[n].Dispose();
		}

		reader.Dispose();
	}
}
