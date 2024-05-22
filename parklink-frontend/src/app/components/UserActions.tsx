"use client";

import { Button, Dropdown } from "flowbite-react";
import { User } from "next-auth";
import Link from "next/link";
import { signOut } from "next-auth/react";
import * as React from 'react';
import Box from '@mui/material/Box';
import Avatar from '@mui/material/Avatar';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import ListItemIcon from '@mui/material/ListItemIcon';
import Divider from '@mui/material/Divider';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import Tooltip from '@mui/material/Tooltip';
import Logout from '@mui/icons-material/Logout';
import { Badge } from "@/components/ui/badge"

type Props = {
    user: Partial<User>; // Partial means that the type is optional. Doesn't have to be a full user.
    role: any;
}

export default function UserActions({ user, role }: Props) {

    // This is a dictionary of dropdown items. The key is the role, 
    //and the value is an array of dropdown items.
    let dropDownItems: {[key: string]: {name: string, link: string}[]} = {
        "user": [
            {name: "Profile", link: "/account"},
            {name: "My bookings", link: "/account/bookings"},
            {name: "Session (Dev only)", link: "/account/admin/session"},
        ],
        "admin": [
            {name: "Profile", link: "/account"},
            {name: "Admin Panel", link: "/account/admin/panel"},
            {name: "My bookings", link: "/account/bookings"},
            {name: "Session (Dev only)", link: "/account/admin/session"},
        ],
        "provider": [
            {name: "Profile", link: "/account"},
            {name: "Provider Panel", link: "/account/provider/panel"},
            {name: "My bookings", link: "/account/bookings"},
            {name: "Session (Dev only)", link: "/account/admin/session"},
        ]
    }

    const [anchorEl, setAnchorEl] = React.useState(null);
    const open = Boolean(anchorEl);
    const handleClick = (event: any) => {
        setAnchorEl(event.currentTarget);
    };
    const handleClose = () => {
        setAnchorEl(null);
    };

    var userNameForDropdown = user.name ?? '';

    return (
        <>
        <Tooltip title="Account Menu">
            <IconButton
                onClick={handleClick}
                size="small"
                sx={{ ml: 2, mr: 2 }}
                aria-controls={open ? 'account-menu' : undefined}
                aria-haspopup="true"
                aria-expanded={open ? 'true' : undefined}
            >
                <Avatar sx={{width: 48, height: 48}} src={"https://ui-avatars.com/api/?background=0D8ABC&color=fff&name=" + userNameForDropdown}>
                </Avatar>
            </IconButton>
        </Tooltip>
        <Menu
            anchorEl={anchorEl}
            id="account-menu"
            open={open}
            onClose={handleClose}
            onClick={handleClose}
            PaperProps={{
                elevation: 0,
                sx: {
                  overflow: 'visible',
                  filter: 'drop-shadow(0px 2px 8px rgba(0,0,0,0.32))',
                  mt: 1.5,
                  '& .MuiAvatar-root': {
                    width: 32,
                    height: 32,
                    ml: -0.5,
                    mr: 1,
                  },
                  '&::before': {
                    content: '""',
                    display: 'block',
                    position: 'absolute',
                    top: 0,
                    right: 14,
                    width: 10,
                    height: 10,
                    bgcolor: 'background.paper',
                    transform: 'translateY(-50%) rotate(45deg)',
                    zIndex: 0,
                  },
                },
            }}
            transformOrigin={{ horizontal: 'right', vertical: 'top' }}
            anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
        >
            <MenuItem>
            <Avatar src={"https://ui-avatars.com/api/?background=0D8ABC&color=fff&name=" + userNameForDropdown} />
            <Typography variant="subtitle1" className="antialiased">{userNameForDropdown}</Typography>
            {role == "admin" || role == "provider" ? (
                <Badge variant="outline" className="ml-2">{role}</Badge>
            ) : null}
            </MenuItem>
            <Divider />
            {
                dropDownItems[role].map((item, index) => (
                    <Link key={index} href={item.link}>
                        <MenuItem key={index} className="font-medium antialiased">
                            {item.name}
                        </MenuItem>
                    </Link>
                ))
            }
            <Divider />
            <MenuItem onClick={() => signOut({callbackUrl: '/'})}>
                <ListItemIcon>
                    <Logout fontSize="small" />
                </ListItemIcon>
                <Typography variant="inherit">Sign out</Typography>
            </MenuItem>
        </Menu>
        </>
    )
}