# -*- coding: utf-8 -*-
import json
import re

path = r"e:\Yaer\yaer\Yaer\Assets\GameRes\Prefabs\Dialogue\ChapterEndStory_0.prefab"
text = open(path, encoding="utf-8").read()

# YAML single-quoted JSON blob
m = re.search(r"_boundGraphSerialization:\s*'(.+)'\s*$", text, re.M)
if not m:
    # sometimes double-quoted and long line
    m = re.search(r'_boundGraphSerialization:\s*"(.*)"\s*$', text, re.M)
raw = m.group(1)
# Unity YAML single-quote escaping: '' -> '
raw = raw.replace("''", "'")
data = json.loads(raw)
nodes = data.get("nodes", [])
print("node_count", len(nodes))
for i, n in enumerate(nodes):
    t = n.get("$type", "")
    act = n.get("_action")
    parts = [f"[{i}] {t.split('.')[-1]}"]
    if act:
        at = act.get("$type", "")
        parts.append(f"action={at.split('.')[-1]}")
        if "chapterId" in act:
            cid = act["chapterId"]
            if isinstance(cid, dict):
                parts.append(f"chapterId={cid}")
            else:
                parts.append(f"chapterId={cid}")
        if "actions" in act:
            for j, a in enumerate(act["actions"]):
                at2 = a.get("$type", "").split(".")[-1]
                extras = []
                for k, v in a.items():
                    if k == "$type":
                        continue
                    if isinstance(v, dict) and "_value" in v:
                        extras.append(f"{k}={v['_value']}")
                    elif k in (
                        "setTo",
                        "chapterId",
                        "NextSceneName",
                        "sceneName",
                        "gameObjectName",
                        "childName",
                    ):
                        extras.append(f"{k}={v}")
                parts.append(f"  sub[{j}]={at2} {' '.join(str(x) for x in extras)}")
    # statement text
    for key in ("_statement", "statement"):
        if key in n:
            st = n[key]
            if isinstance(st, dict):
                txt = st.get("_text") or st.get("text") or ""
                if txt:
                    parts.append(f"text={txt[:80]}")
    print(" | ".join(parts)[:500])

# connections / all types
types = {}
blob = json.dumps(data)
for needle in [
    "ChapterEndAction",
    "LoadScene",
    "chapterId",
    "Village",
    "ForestEast",
    "MapPanel",
    "OpenUIForm",
]:
    print(f"contains {needle}:", needle in blob)

# find ChapterEndAction specifically
for i, n in enumerate(nodes):
    s = json.dumps(n)
    if "ChapterEnd" in s or "LoadScene" in s or "chapterId" in s:
        print("--- INTERESTING NODE", i, "---")
        print(s[:1200])
        print("...")
