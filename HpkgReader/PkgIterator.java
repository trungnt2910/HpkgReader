/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

package org.haiku.pkg;

import com.google.common.base.Preconditions;
import org.haiku.pkg.model.Attribute;
import org.haiku.pkg.model.Pkg;

/**
 * <p>This object will wrap an attribute iterator to be able to generate a series of {@link Pkg} objects that
 * model a package in the Haiku package management system.</p>
 */

public class PkgIterator {

    private final AttributeIterator attributeIterator;
    private final PkgFactory pkgFactory;

    public PkgIterator(AttributeIterator attributeIterator) {
        this(attributeIterator, new PkgFactory());
    }

    private PkgIterator(AttributeIterator attributeIterator, PkgFactory pkgFactory) {
        super();
        Preconditions.checkNotNull(attributeIterator);
        this.attributeIterator = attributeIterator;
        this.pkgFactory = pkgFactory;
    }

    /**
     * <p>This method will return true if there are more packages to be obtained from the attributes iterator.</p>
     */

    public boolean hasNext() {
        return attributeIterator.hasNext();
    }

    /**
     * <p>This method will return the next package from the attribute iterator supplied.</p>
     * @return The return value is the next package from the list of attributes.
     * @throws PkgException when there is a problem obtaining the next package from the attributes.
     * @throws HpkException when there is a problem obtaining the next attributes.
     */

    public Pkg next() {
        Attribute attribute = attributeIterator.next();

        if (null != attribute) {
            return pkgFactory.createPackage(attributeIterator.getContext(), attribute);
        }

        return null;
    }

}
