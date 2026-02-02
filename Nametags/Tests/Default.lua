-- Note: unlike the Default nametag, this doesn't actually grant any custom icons, so I can
-- understand how I want Lua integration to work in the future

-- example config
local config = {
    SanitizeNicknames = true,
    Icons = true,
    PlatformIcons = true
}

local platformCache = { }

local function getPlatform(player)
    if platformCache[player.id] ~= nil then
        return {platformCache[player.id], true}
    end

    local platform_string = "meta"
    local sure = true

    if player.rigraw["currentRankedSubTierQuest"] > 0 then
        goto platform_save
    end

    if table.find(player.cosmetics, "s. first login") or player.rigraw["currentRankedSubTierPC"] > 0 then
        platform_string = "steam"
        goto platform_save
    end

    if table.find(player.cosmetics, "first login") or player.rigraw["currentRankedSubTierPC"] > 0 or #player.properties > 1 then
        platform_string = "oculus"
        goto platform_save
    end

    sure = #player.cosmetics > 0

    ::platform_save::
    if sure then
        platformCache[player.id] = platform_string
    end

    return {platform_string, sure}
end

local function updateNametag(nametag)
    local prefix = ""

    if config.Icons and config.PlatformIcons then
        local platformResult = getPlatform(nametag.owner)

        prefix = string.format("<sprite name=\"%s\">", platformResult[1])

        if platformResult[2] then
            prefix = prefix .. "? "
        end
    end

    nametag.text = string.format("%s%s", prefix, nametag.owner.nickname)
end

return {
    name = "Default",
    author = "Bingus",
    description = "The default nametag provided by BingusNametags++. Includes platform icons and a nametag.",

    nametags = {
        {
            name = "Default",
            offset = 0,
            update = updateNametag
        }
    }
}