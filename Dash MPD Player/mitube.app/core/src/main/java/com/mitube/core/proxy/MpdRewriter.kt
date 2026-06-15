package com.mitube.core.proxy

import com.mitube.core.models.DrmInfo
import java.io.StringReader
import java.nio.ByteBuffer
import javax.xml.parsers.DocumentBuilderFactory
import org.w3c.dom.Document
import org.w3c.dom.NodeList
import org.xml.sax.InputSource

class MpdRewriter(private val proxyBaseUrl: String) {

    companion object {
        private val DRM_SYSTEMS = mapOf(
            "urn:uuid:edef8ba9-79d6-4ace-a3c8-27dcd51d21ed" to DrmInfo.WIDEVINE_UUID,
            "urn:uuid:9a04f079-9840-4286-ab92-e65be0885f95" to DrmInfo.PLAYREADY_UUID,
            "urn:uuid:e2719d58-a985-b3c9-781a-b030af78d30e" to DrmInfo.CLEARKEY_UUID
        )
    }

    fun rewrite(mpdXml: String, token: String): String {
        return try {
            rewriteWithXml(mpdXml, token)
        } catch (_: Exception) {
            rewriteWithRegex(mpdXml, token)
        }
    }

    fun detectDrm(mpdXml: String): DrmInfo? {
        return try {
            val doc = parseXml(mpdXml)
            val nodes = doc.getElementsByTagNameNS("*", "ContentProtection")
            for (i in 0 until nodes.length) {
                val cp = nodes.item(i)
                val schemeAttr = cp.attributes?.getNamedItem("schemeIdUri")?.nodeValue ?: continue
                val systemId = DRM_SYSTEMS[schemeAttr] ?: continue
                val licenseUrl = extractLicenseUrl(cp) ?: continue
                return DrmInfo(systemId, licenseUrl)
            }
            null
        } catch (_: Exception) {
            null
        }
    }

    private fun extractLicenseUrl(cp: org.w3c.dom.Node): String? {
        val children = cp.childNodes

        // Try dashif:laurl
        for (i in 0 until children.length) {
            val child = children.item(i)
            if (child.localName?.equals("laurl", ignoreCase = true) == true) {
                val ns = child.namespaceURI
                val text = child.textContent?.takeIf { it.isNotBlank() }
                if (text != null && (ns?.contains("dashif") == true || ns?.contains("playready") == true)) {
                    return text
                }
            }
        }

        // Try mspr:pro (PlayReady Object binary)
        for (i in 0 until children.length) {
            val child = children.item(i)
            if (child.localName?.equals("pro", ignoreCase = true) == true) {
                val base64 = child.textContent
                if (!base64.isNullOrBlank()) {
                    val url = extractLicenseFromPlayReadyObject(base64)
                    if (url != null) return url
                }
            }
        }

        return null
    }

    private fun extractLicenseFromPlayReadyObject(base64Pro: String): String? {
        return try {
            val bytes = android.util.Base64.decode(base64Pro, android.util.Base64.DEFAULT)
            if (bytes.size < 10) return null

            val recordCount = ((bytes[4].toInt() and 0xFF) shl 8) or (bytes[5].toInt() and 0xFF)
            var offset = 6

            for (i in 0 until recordCount) {
                if (offset + 4 > bytes.size) break
                val type = ((bytes[offset].toInt() and 0xFF) shl 8) or (bytes[offset + 1].toInt() and 0xFF)
                val length = ((bytes[offset + 2].toInt() and 0xFF) shl 8) or (bytes[offset + 3].toInt() and 0xFF)
                offset += 4

                if (type != 1 || offset + length > bytes.size) {
                    offset += length
                    continue
                }

                val xml = String(bytes, offset, length, Charsets.UTF_16LE)
                val laUrlStart = xml.indexOf("<LA_URL>", ignoreCase = true)
                if (laUrlStart < 0) break
                val contentStart = laUrlStart + 8
                val laUrlEnd = xml.indexOf("</LA_URL>", contentStart, ignoreCase = true)
                if (laUrlEnd < 0) break

                return xml.substring(contentStart, laUrlEnd)
            }
            null
        } catch (_: Exception) {
            null
        }
    }

    private fun rewriteWithXml(mpdXml: String, token: String): String {
        val doc = parseXml(mpdXml)

        // Rewrite SegmentTemplate @media and @initialization
        val segNodes = doc.getElementsByTagNameNS("*", "SegmentTemplate")
        for (i in 0 until segNodes.length) {
            val seg = segNodes.item(i)
            val attrs = seg.attributes

            val media = attrs.getNamedItem("media")
            if (media != null) {
                val path = getFileNameFromUrl(media.nodeValue)
                media.nodeValue = "$proxyBaseUrl/segment/$token/$path"
            }

            val init = attrs.getNamedItem("initialization")
            if (init != null) {
                val path = getFileNameFromUrl(init.nodeValue)
                init.nodeValue = "$proxyBaseUrl/segment/$token/$path"
            }
        }

        // Rewrite BaseURL elements
        val baseUrlNodes = doc.getElementsByTagNameNS("*", "BaseURL")
        for (i in 0 until baseUrlNodes.length) {
            val baseUrlEl = baseUrlNodes.item(i)
            val path = getFileNameFromUrl(baseUrlEl.textContent)
            baseUrlEl.textContent = "$proxyBaseUrl/segment/$token/$path"
        }

        return xmlToString(doc)
    }

    private fun rewriteWithRegex(mpdXml: String, token: String): String {
        val mediaRegex = Regex("""(media|initialization)\s*=\s*"([^"]+)"""")
        val baseUrlRegex = Regex("<BaseURL>([^<]+)</BaseURL>")

        var result = mediaRegex.replace(mpdXml) { match ->
            val attr = match.groupValues[1]
            val path = getFileNameFromUrl(match.groupValues[2])
            """$attr="$proxyBaseUrl/segment/$token/$path""""
        }

        result = baseUrlRegex.replace(result) { match ->
            val path = getFileNameFromUrl(match.groupValues[1])
            "<BaseURL>$proxyBaseUrl/segment/$token/$path</BaseURL>"
        }

        return result
    }

    private fun getFileNameFromUrl(url: String): String {
        if (!url.startsWith("http")) return url
        val trimmed = url.trimEnd('/')
        val lastSlash = trimmed.lastIndexOf('/')
        return if (lastSlash >= 0) trimmed.substring(lastSlash + 1) else trimmed
    }

    private fun parseXml(xml: String): Document {
        val factory = DocumentBuilderFactory.newInstance().also {
            it.isNamespaceAware = true
        }
        val builder = factory.newDocumentBuilder()
        return builder.parse(InputSource(StringReader(xml)))
    }

    private fun xmlToString(doc: Document): String {
        val transformer = javax.xml.transform.TransformerFactory.newInstance().newTransformer()
        transformer.setOutputProperty(javax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "yes")
        transformer.setOutputProperty(javax.xml.transform.OutputKeys.INDENT, "no")
        val writer = java.io.StringWriter()
        transformer.transform(
            javax.xml.transform.dom.DOMSource(doc),
            javax.xml.transform.stream.StreamResult(writer)
        )
        return writer.toString()
    }
}
