using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Data.Json;

namespace LightMusic.Services {
    static class MusicService {
        private static string DefaultUrl = "ms-appx:///Assets/music.png";
        private static string GetAlbumUrl = "http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.album.getAlbumInfo&format=json&album_id=$$$$$";
        private static string SearchSongUrl = "http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.search.common&page_size=30&page_no=1&format=json&query=$$$$$";
        private static string GetSongUrl = "http://music.baidu.com/data/music/links?songIds=$$$$$";

        public static async Task<JsonObject> SearchSong(string name) {
            return await ApiService.GetJson(SearchSongUrl.Replace("$$$$$", name));
        }

        public static async Task<JsonObject> GetAlbumDetail(string id) {
            return await ApiService.GetJson(GetAlbumUrl.Replace("$$$$$", id));
        }

        public static async Task<JsonObject> GetSongDetail(string id) {
            return await ApiService.GetJson(GetSongUrl.Replace("$$$$$", id));
        }

        public static string GetAlbumTitleFromSong(JsonObject song) {
            try {
                JsonArray songList = song["data"].GetObject()["songList"].GetArray();
                if (songList.Count == 0) {
                    return "未知";
                } else {
                    JsonObject songObject = songList.GetObjectAt(0);
                    return songObject["albumName"].GetString();
                }
            } catch {
                return "未知";
            }
        }

        public static string GetSongIdFromSearch(JsonObject data) {
            try {
                JsonArray songList = data["song_list"].GetArray();
                if (songList.Count == 0) {
                    return null;
                } else {
                    JsonObject songObject = songList.GetObjectAt(0);
                    return songObject["song_id"].GetString();
                }
            } catch {
                return null;
            }
        }

        public static string GetAlbumImageFromSong(JsonObject song) {
            try {
                JsonArray songList = song["data"].GetObject()["songList"].GetArray();
                if (songList.Count == 0) {
                    return DefaultUrl;
                } else {
                    JsonObject songObject = songList.GetObjectAt(0);
                    return songObject["songPicRadio"].GetString();
                }
            } catch {
                return DefaultUrl;
            }
        }
    }
}
