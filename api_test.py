#!/usr/bin/env python3
"""
自动化接口排查脚本
1. 检查 swagger 可达
2. 注册随机邮箱
3. 登录获取 JWT
4. 添加购物车 → 查询 → 删除
运行：python api_test.py http://localhost:5000
"""
import json
import random
import string
import sys
from typing import Tuple

import requests

BASE_URL = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5000"
API = f"{BASE_URL.rstrip('/')}/api"


def log(step: str, ok: bool, detail: str = ""):
    status = "✅" if ok else "❌"
    print(f"{status} {step} {detail}")


def rand_email() -> str:
    suffix = "".join(random.choices(string.ascii_lowercase + string.digits, k=6))
    return f"user_{suffix}@demo.com"


def check_swagger() -> bool:
    resp = requests.get(f"{BASE_URL.rstrip('/')}/swagger/index.html")
    return resp.status_code == 200


def register_user(email: str, pwd: str) -> bool:
    resp = requests.post(
        f"{API}/auth/register",
        json={"email": email, "password": pwd},
        timeout=10,
    )
    return resp.ok


def login(email: str, pwd: str) -> Tuple[str, dict]:
    resp = requests.post(
        f"{API}/auth/login",
        json={"identifier": email, "password": pwd},
        timeout=10,
    )
    if not resp.ok:
        return "", {}
    data = resp.json()
    return data["token"], data["user"]


def add_cart(token: str, pid: int, qty: int = 1) -> bool:
    resp = requests.post(
        f"{API}/cart/add",
        headers={"Authorization": f"Bearer {token}"},
        json={"productId": pid, "quantity": qty},
    )
    return resp.ok


def get_cart(token: str):
    resp = requests.get(
        f"{API}/cart",
        headers={"Authorization": f"Bearer {token}"},
    )
    return resp.ok, resp.json() if resp.ok else None


def del_cart(token: str, pid: int) -> bool:
    resp = requests.delete(
        f"{API}/cart/{pid}",
        headers={"Authorization": f"Bearer {token}"},
    )
    return resp.ok


if __name__ == "__main__":
    print(f"开始接口排查，BaseURL = {BASE_URL}")

    # 1. Swagger
    ok = check_swagger()
    log("Swagger 可达", ok)

    # 2. 注册 & 登录
    email = rand_email()
    pwd = "Test1234"
    ok_reg = register_user(email, pwd)
    log("用户注册", ok_reg, email)

    token, user = login(email, pwd)
    ok_login = bool(token)
    log("用户登录", ok_login)

    # 3. 购物车流程
    if ok_login:
        pid = 101
        ok_add = add_cart(token, pid, 2)
        log("加入购物车", ok_add)

        ok_get, items = get_cart(token)
        log("查询购物车", ok_get, json.dumps(items, ensure_ascii=False))

        ok_del = del_cart(token, pid)
        log("删除购物车项", ok_del)

        ok_get2, items2 = get_cart(token)
        log("再次查询购物车应为空", ok_get2 and not items2)

    print("排查完成")

