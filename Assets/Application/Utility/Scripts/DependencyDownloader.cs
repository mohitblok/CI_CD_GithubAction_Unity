using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;


/// <summary>
/// Downloads dependencies for a given environment
/// </summary>
public class DependencyDownloader
{
    /// <summary> The DownloadDependencies function downloads the dependencies for an environment data entry.</summary>
    /// <param name="environmentDataEntry"> The environment data entry to download dependencies for.</param>
    /// <param name="onComplete">called when files have been checked and downloaded</param>
    public static void DownloadDependencies(EnvironmentDataEntry environmentDataEntry, Action onComplete)
    {
        List<MediaEntry> dependencies = new List<MediaEntry>();

        CalculateLocationDependencies(environmentDataEntry, list =>
        {
            dependencies.AddRange(list);
            CalculateSceneGraphDependencies(environmentDataEntry, entries =>
            {
                dependencies.AddRange(entries);

                List<string> assetsToDownload = new List<string>();

                TaskAction hashCheckTask = new TaskAction(dependencies.Count, () =>
                {
                    if (assetsToDownload.Count == 0)
                    {
                        onComplete?.Invoke();
                    }
                    else
                    {
                        DownloadMissingDependencies(assetsToDownload, onComplete);
                    }
                });

                foreach (var dependency in dependencies)
                {
                    var path = DomainManager.Instance.GetLocalAssetBundle(dependency.assetData.downloadData);
                    var hash = string.Empty;
                    var platform = DomainManager.GetPlatform();
                    if (dependency.assetData.assetHashDict.ContainsKey(platform))
                    {
                        hash = dependency.assetData.assetHashDict[DomainManager.GetPlatform()];
                    }

                    HashChecker.HashCheckFileAsync(path, hash, () => { hashCheckTask.Increment(); },
                        () =>
                        {
                            Debug.LogError($"Failed to hash check at path: {path}");
                            if (IO.FileExists(path))
                            {
                                IO.DeleteFile(path);
                            }

                            assetsToDownload.Add(dependency.assetData.downloadData);
                            hashCheckTask.Increment();
                        });
                }
            });
        });
    }

    private static void CalculateLocationDependencies(EnvironmentDataEntry environmentDataEntry,
        Action<List<MediaEntry>> onComplete)
    {
        ContentManager.Instance.GetData(environmentDataEntry.location, entry =>
        {
            var location = (LocationDataEntry)entry;
            var dependencies = new List<MediaEntry>();

            TaskAction task = new TaskAction(location.scenes.Count, () => { onComplete?.Invoke(dependencies); });

            foreach (var scene in location.scenes)
            {
                ContentManager.Instance.GetData(scene, dataEntry =>
                {
                    var mediaEntry = (MediaEntry)dataEntry;
                    bool willAdd = true;
                    foreach (var dependency in dependencies)
                    {
                        if (dependency.assetData.downloadData == mediaEntry.assetData.downloadData)
                        {
                            willAdd = false;
                        }
                    }

                    if (willAdd)
                    {
                        dependencies.Add(mediaEntry);
                    }

                    task.Increment();
                }, s =>
                {
                    Debug.Log($"Failed to load scene item {scene} - {s}");
                    task.Increment();
                });
            }
        }, s => { Debug.Log($"Failed to load location {environmentDataEntry.location} - {s}"); });
    }

    private static void CalculateSceneGraphDependencies(EnvironmentDataEntry environmentDataEntry,
        Action<List<MediaEntry>> onComplete)
    {
        ContentManager.Instance.GetData(environmentDataEntry.sceneGraph, entry =>
        {
            var sceneGraph = (SceneGraphEntry)entry;
            var dependencies = new List<MediaEntry>();

            var taskCount = 0;

            List<ContentDataEntry> sceneContentDatas = new List<ContentDataEntry>();

            TaskAction dependencyCounterTask = new TaskAction(sceneGraph.sceneItems.Count, () =>
            {
                TaskAction task = new TaskAction(taskCount,
                    () => { onComplete?.Invoke(dependencies); });

                foreach (var contentData in sceneContentDatas)
                {
                    if (contentData is TemplateDataEntry templateDataEntry)
                    {
                        foreach (var templateDependency in templateDataEntry.dependencyList)
                        {
                            bool willAdd = true;
                            foreach (var dependency in dependencies)
                            {
                                if (dependency.guid == templateDependency)
                                {
                                    willAdd = false;
                                }
                            }

                            if (willAdd)
                            {
                                ContentManager.Instance.GetData(templateDependency,
                                    dataEntry =>
                                    {
                                        dependencies.Add((MediaEntry)dataEntry);
                                        task.Increment();
                                    },
                                    s =>
                                    {
                                        Debug.Log(
                                            $"Failed to load templateDataEntry.guid {templateDataEntry.guid} - {s}");
                                        task.Increment();
                                    });
                            }
                            else
                            {
                                task.Increment();
                            }
                        }
                    }
                    else
                    {
                        var mediaEntry = (MediaEntry)contentData;
                        bool willAdd = true;
                        foreach (var dependency in dependencies)
                        {
                            if (dependency.assetData.downloadData == mediaEntry.assetData.downloadData)
                            {
                                willAdd = false;
                            }
                        }

                        if (willAdd)
                        {
                            dependencies.Add(mediaEntry);
                        }

                        task.Increment();
                    }
                }
            });
            
            foreach (var sceneItem in sceneGraph.sceneItems)
            {
                ContentManager.Instance.GetData(sceneItem.contentGuid, contentData =>
                {
                    if (contentData is TemplateDataEntry templateDataEntry)
                    {
                        taskCount += templateDataEntry.dependencyList.Count;
                    }
                    else
                    {
                        taskCount++;
                    }

                    sceneContentDatas.Add(contentData);
                    dependencyCounterTask.Increment();
                }, s =>
                {
                    dependencyCounterTask.Increment();
                    Debug.LogError(s);
                });
            }
        }, s => { Debug.Log($"Failed to load sceneGraph {environmentDataEntry.sceneGraph} - {s}"); });
    }

    private static void DownloadMissingDependencies(List<string> downloadDatas, Action onComplete)
    {
        TaskAction downloadTask = new TaskAction(downloadDatas.Count, () => { onComplete?.Invoke(); });

        foreach (var data in downloadDatas)
        {
            var url = DomainManager.Instance.GetAssetBundleData(data);
            var path = DomainManager.Instance.GetLocalAssetBundle(data);


            WebRequestManager.Instance.DownloadSmallItem<Dictionary<string, string>>(url, (dict) =>
            {
                var key = $"{DomainManager.GetPlatform().ToLower()}AssetPath";
                var assetBundleUrl = dict[key];

                WebRequestManager.Instance.DownloadLargeItem(assetBundleUrl, path,
                    () =>
                    {
                        Debug.Log($"{url} downloaded to {path}");
                        downloadTask.Increment();
                    },
                    error =>
                    {
                        Debug.LogError($"Error downloading missing dependency: {url} - {error}");
                        downloadTask.Increment();
                    });
            }, s =>
            {
                Debug.LogError($"Error downloading asset bundle data: {url} - {s}");
                downloadTask.Increment();
            });
        }
    }
}